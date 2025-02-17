using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Products.Application.Identity.Interfaces;
using Products.Common;
using Products.Infrastructure.Identity;
using Products.Infrastructure.Persistence;
using Xunit;

namespace Products.Tests
{
    public class IdentityServiceTests
    {
        private readonly IIdentityService _identityService;
        private readonly DbContextOptions<UserDbContext> _options;
        private const string USERS_DATABASE_TEST = "TestIdentityDb";
        public IdentityServiceTests()
        {
            var serviceProvider = SetupTestServiceProvider();

            _identityService = serviceProvider.GetRequiredService<IIdentityService>();

            _options = Helper.GetInMemoryOptions<UserDbContext>(USERS_DATABASE_TEST);
        }

        [Fact]
        public async Task CreateUserAsync_CreatesAUserInTheDatabase()
        {
            //Arrange
            string userName = "UserCreationTest";
            string password = "PasswordTest#123";
            string role = "Admin";

            //Act
            await _identityService.CreateUserAsync(userName, password, role);

            using (var context = new UserDbContext(_options))
            {
                var isUserCreated = context.Users.Any(u => u.UserName == userName);

                //Assert
                Assert.True(isUserCreated);
            }
        }

        private ServiceProvider SetupTestServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddDbContext<UserDbContext>(options =>
                options.UseInMemoryDatabase(USERS_DATABASE_TEST));

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<UserDbContext>()
                .AddDefaultTokenProviders();

            services.AddScoped<IIdentityService, IdentityService>();

            services.AddLogging();

            var serviceProvider = services.BuildServiceProvider();

            return serviceProvider;
        }
    }
}
