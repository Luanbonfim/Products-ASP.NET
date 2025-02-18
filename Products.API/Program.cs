using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Products.Application.Interfaces;
using Products.Application.Services_;
using Products.Common.Helpers;
using Products.Infrastructure.Identity;
using Products.Infrastructure.Messaging;
using Products.Infrastructure.Persistence;
using Products.Infrastructure.Repositories;
using Products.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register DbContext with SQL Lite
builder.Services.AddDbContext<ProductsDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<UserDbContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

//// Add Cookies based Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
           .AddCookie();

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
});

// Add controllers
builder.Services.AddControllers();

//DIs
builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddScoped<IIdentityService, IdentityService>();

builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

builder.Logging.AddConsole();

builder.Services.AddLogging();

builder.Services.AddSingleton<LoggerHelper>();

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<UserDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
});

var app = builder.Build();

// Use Swagger UI middleware
app.UseSwagger();  // Adds Swagger JSON endpoint
app.UseSwaggerUI();  // Adds Swagger UI for browsing the API

app.UseHttpsRedirection();

app.UseRouting();

// Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Use the logging middleware
app.UseMiddleware<LoggingMiddleware>();

// Map controllers
app.MapControllers();

app.Run();