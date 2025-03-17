using Products.Middlewares;
using System.Security.Cryptography.X509Certificates;

namespace Products.API.Extensions
{
    public static class WebApplicationExtensions
    {
        public static WebApplication ConfigureWebApplication(this WebApplication app)
        {
            // Use Swagger UI middleware
            app.UseSwagger();
            app.UseSwaggerUI();

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

            return app;
        }

        public static WebApplicationBuilder ConfigureKestrelWithCertificates(this WebApplicationBuilder builder)
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

            return builder;
        }

        public static WebApplicationBuilder ConfigureUrls(this WebApplicationBuilder builder)
        {
            builder.WebHost.UseUrls("http://0.0.0.0:5209", "https://0.0.0.0:7263");
            return builder;
        }
    }
} 