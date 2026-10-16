using Microsoft.EntityFrameworkCore;
using RaceDay.Core.DTOs;
using RaceDay.Infrastructure.Data;
using RaceDay.Infrastructure.Services;
using Xunit;

namespace RaceDay.Tests
{
    public class EventTests
    {
        private RaceDayDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDbContext(options);
        }

        [Fact]
        public async Task CreateEvent_WithValidData_ReturnsEvent()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            await SeedOrganiser(context);
            var service = new EventService(context);

            var createDto = new CreateEventDto
            {
                Title = "Test Marathon",
                Description = "A test marathon",
                StartDate = DateTime.UtcNow.AddDays(30),
                EndDate = DateTime.UtcNow.AddDays(30).AddHours(8),
                Location = "Test Location",
                City = "Cape Town",
                Province = "Western Cape",
                EventType = "Running",
                MaxParticipants = 1000
            };

            // Act
            var result = await service.CreateEventAsync(1, createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Marathon", result.Title);
            Assert.Equal("Cape Town", result.City);
        }

        [Fact]
        public async Task GetAllEvents_ReturnsEvents()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            await SeedOrganiser(context);
            var service = new EventService(context);

            await service.CreateEventAsync(1, new CreateEventDto
            {
                Title = "Event 1",
                StartDate = DateTime.UtcNow.AddDays(30),
                EndDate = DateTime.UtcNow.AddDays(30).AddHours(8),
                Location = "Loc 1",
                City = "Cape Town",
                Province = "Western Cape",
                EventType = "Running",
                MaxParticipants = 100
            });

            // Act
            var result = await service.GetAllEventsAsync(null, null);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        private async Task SeedOrganiser(RaceDayDbContext context)
        {
            var user = new Core.Entities.User
            {
                UserID = 1,
                Email = "organiser@test.com",
                PasswordHash = "hash",
                FirstName = "Test",
                LastName = "Organiser",
                Role = "Organiser"
            };
            context.Users.Add(user);

            var organiser = new Core.Entities.Organiser
            {
                OrganiserID = 1,
                UserID = 1,
                Organisation = "Test Org"
            };
            context.Organisers.Add(organiser);
            await context.SaveChangesAsync();
        }
    }
}