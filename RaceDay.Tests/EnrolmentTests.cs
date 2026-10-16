using Microsoft.EntityFrameworkCore;
using RaceDay.Core.DTOs;
using RaceDay.Infrastructure.Data;
using RaceDay.Infrastructure.Services;
using Xunit;

namespace RaceDay.Tests
{
    public class EnrolmentTests
    {
        private RaceDayDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDbContext(options);
        }

        [Fact]
        public async Task CreateEnrolment_WithValidData_ReturnsEnrolment()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            await SeedData(context);
            var service = new EnrolmentService(context);

            // Act
            var result = await service.CreateEnrolmentAsync(2, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Confirmed", result.Status);
        }

        [Fact]
        public async Task CreateEnrolment_DuplicateEnrolment_ReturnsNull()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            await SeedData(context);
            var service = new EnrolmentService(context);

            await service.CreateEnrolmentAsync(2, 1);

            // Act
            var result = await service.CreateEnrolmentAsync(2, 1);

            // Assert
            Assert.Null(result);
        }

        private async Task SeedData(RaceDayDbContext context)
        {
            // Organiser user
            context.Users.Add(new Core.Entities.User
            {
                UserID = 1,
                Email = "organiser@test.com",
                PasswordHash = "hash",
                FirstName = "Test",
                LastName = "Organiser",
                Role = "Organiser"
            });

            // Participant user
            context.Users.Add(new Core.Entities.User
            {
                UserID = 2,
                Email = "participant@test.com",
                PasswordHash = "hash",
                FirstName = "Test",
                LastName = "Participant",
                Role = "Participant"
            });

            context.Organisers.Add(new Core.Entities.Organiser
            {
                OrganiserID = 1,
                UserID = 1
            });

            context.Participants.Add(new Core.Entities.Participant
            {
                ParticipantID = 1,
                UserID = 2
            });

            context.Events.Add(new Core.Entities.Event
            {
                EventID = 1,
                OrganiserID = 1,
                Title = "Test Event",
                StartDate = DateTime.UtcNow.AddDays(30),
                EndDate = DateTime.UtcNow.AddDays(30).AddHours(8),
                Location = "Test",
                City = "Test",
                Province = "Test",
                EventType = "Running",
                MaxParticipants = 100
            });

            context.Categories.Add(new Core.Entities.Category
            {
                CategoryID = 1,
                EventID = 1,
                CategoryName = "10km",
                Distance = 10,
                EntryFee = 100,
                MaxParticipants = 50
            });

            await context.SaveChangesAsync();
        }
    }
}