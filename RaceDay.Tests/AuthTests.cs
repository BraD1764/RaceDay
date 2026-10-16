using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using RaceDay.Core.DTOs;
using RaceDay.Infrastructure.Data;
using RaceDay.Infrastructure.Services;
using Xunit;

namespace RaceDay.Tests
{
    public class AuthTests
    {
        private RaceDayDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDbContext(options);
        }

        private IConfiguration GetConfiguration()
        {
            var config = new Mock<IConfiguration>();
            config.Setup(c => c["Jwt:Secret"]).Returns("TestSecretKeyThatIsAtLeast32CharactersLongForUnitTesting!");
            return config.Object;
        }

        [Fact]
        public async Task Register_WithValidData_ReturnsAuthResponse()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new AuthService(context, GetConfiguration());
            var registerDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "Password123!",
                FirstName = "Test",
                LastName = "User",
                Role = "Participant"
            };

            // Act
            var result = await service.RegisterAsync(registerDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test@example.com", result.Email);
            Assert.Equal("Participant", result.Role);
            Assert.NotEmpty(result.Token);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ReturnsNull()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new AuthService(context, GetConfiguration());
            var registerDto = new RegisterDto
            {
                Email = "test@example.com",
                Password = "Password123!",
                FirstName = "Test",
                LastName = "User",
                Role = "Participant"
            };

            await service.RegisterAsync(registerDto);

            // Act
            var result = await service.RegisterAsync(registerDto);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsAuthResponse()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new AuthService(context, GetConfiguration());

            await service.RegisterAsync(new RegisterDto
            {
                Email = "test@example.com",
                Password = "Password123!",
                FirstName = "Test",
                LastName = "User",
                Role = "Participant"
            });

            // Act
            var result = await service.LoginAsync(new LoginDto
            {
                Email = "test@example.com",
                Password = "Password123!"
            });

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test@example.com", result.Email);
        }

        [Fact]
        public async Task Login_WithWrongPassword_ReturnsNull()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new AuthService(context, GetConfiguration());

            await service.RegisterAsync(new RegisterDto
            {
                Email = "test@example.com",
                Password = "Password123!",
                FirstName = "Test",
                LastName = "User",
                Role = "Participant"
            });

            // Act
            var result = await service.LoginAsync(new LoginDto
            {
                Email = "test@example.com",
                Password = "WrongPassword"
            });

            // Assert
            Assert.Null(result);
        }
    }
}