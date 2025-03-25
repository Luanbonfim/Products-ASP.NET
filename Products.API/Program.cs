using Microsoft.EntityFrameworkCore;
using Products.API.Extensions;
using Products.Infrastructure.Persistence;

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

// Run migrations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userContext = services.GetRequiredService<UserDbContext>();
        var productsContext = services.GetRequiredService<ProductsDbContext>();
        
        userContext.Database.Migrate();
        productsContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
        throw;
    }
}

app.Run();
