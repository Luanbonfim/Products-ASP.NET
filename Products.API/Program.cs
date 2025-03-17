using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using Products.API.Extensions;
using Products.Infrastructure.Persistence;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services
    .AddApplicationServices()
    .AddDatabaseContexts(builder.Configuration)
    .AddAuthenticationServices()
    .AddIdentityServices()
    .AddSwaggerServices()
    .AddCorsServices()
    .AddApiVersioningServices();

// Configure Kestrel and URLs
builder
    .ConfigureKestrelWithCertificates()
    .ConfigureUrls();

var app = builder.Build();

// Configure the application
app.ConfigureWebApplication();

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