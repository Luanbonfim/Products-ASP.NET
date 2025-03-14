using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Products.Application.Interfaces;
using Products.Common.Helpers;
using Products.Domain.Interfaces;
using Products.Infrastructure.Identity;
using Products.Infrastructure.Messaging;
using Products.Infrastructure.Persistence;
using Products.Infrastructure.Repositories;
using Products.Middlewares;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register DbContext with SQL Lite
builder.Services.AddDbContext<ProductsDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<UserDbContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

//// Add Cookies based Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
           {
               options.ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
               options.ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");
           });

ConfigureSwagger(builder);

// Add controllers
builder.Services.AddControllers();

//DIs
builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.AddScoped<IIdentityService, IdentityService>();

builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

builder.Logging.AddConsole();

builder.Services.AddLogging();

builder.Services.AddSingleton<LoggerHelper>();

ConfigureIdentity(builder);

builder.WebHost.UseUrls("http://0.0.0.0:5209", "https://0.0.0.0:7263");

// Configure Kestrel to use the certificate from environment variables
ConfigureCertificates(builder);

ConfigureCors(builder);

builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true; 
    options.AssumeDefaultVersionWhenUnspecified = true; 
    options.DefaultApiVersion = new ApiVersion(1, 0);
})
.AddMvc();

var app = builder.Build();

// Use Swagger UI middleware
app.UseSwagger();  // Adds Swagger JSON endpoint
app.UseSwaggerUI();  // Adds Swagger UI for browsing the API

app.UseHttpsRedirection();

app.UseCors("AllowAngularApp");
app.UseRouting();

// Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();


// Use the logging middleware
app.UseMiddleware<LoggingMiddleware>();

// Map controllers
app.MapControllers();

app.Run();

void ConfigureCertificates(WebApplicationBuilder builder)
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        var certificatePath = Environment.GetEnvironmentVariable("CERTIFICATE_PATH");
        var certificatePassword = Environment.GetEnvironmentVariable("CERTIFICATE_PASSWORD");

        if (!string.IsNullOrEmpty(certificatePath) && !string.IsNullOrEmpty(certificatePassword))
        {
            options.ConfigureHttpsDefaults(httpsOptions =>
            {
                httpsOptions.ServerCertificate = new X509Certificate2(certificatePath, certificatePassword);
            });
        }
    });
}

void ConfigureCors(WebApplicationBuilder builder)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAngularApp",
            builder =>
            {
                builder.WithOrigins("http://localhost:4200", "https://localhost:7263", "http://localhost:51253", "http://localhost:54143") // Replace with your Angular app's URL
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                       .AllowCredentials(); // Allow credentials (cookies)
            });
    });
}

void ConfigureIdentity(WebApplicationBuilder builder)
{
    builder.Services.AddIdentity<IdentityUser, IdentityRole>()
        .AddEntityFrameworkStores<UserDbContext>()
        .AddDefaultTokenProviders();

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    });
}

void ConfigureSwagger(WebApplicationBuilder builder)
{
    // Add Swagger for API documentation
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

        // Add Cookie Authentication in Swagger
        c.AddSecurityDefinition("cookieAuth", new OpenApiSecurityScheme
        {
            Name = "Cookie",
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Cookie,
            Description = "ASP.NET Identity Cookie Authentication"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "cookieAuth"
                }
            },
            new List<string>()
        }
        });

        c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri("https://accounts.google.com/o/oauth2/auth"),
                    TokenUrl = new Uri("https://oauth2.googleapis.com/token"),
                    Scopes = new Dictionary<string, string>
                    {
                    { "openid", "OpenID scope" },
                    { "profile", "Profile scope" },
                    { "email", "Email scope" }
                    }
                }
            }
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "oauth2"
                }
            },
            new List<string>()
        }
        });
    });
}