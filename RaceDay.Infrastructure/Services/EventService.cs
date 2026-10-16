using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RaceDay.Core.DTOs;
using RaceDay.Core.Entities;
using RaceDay.Core.Interfaces;
using RaceDay.Infrastructure.Data;

namespace RaceDay.Infrastructure.Services
{
    public class EventService : IEventService
    {
        private readonly RaceDayDbContext _context;

        public EventService(RaceDayDbContext context)
        {
            _context = context;
        }

        public async Task<List<EventDto>> GetAllEventsAsync(string? status, string? city)
        {
            var query = _context.Events
                .Include(e => e.Organiser).ThenInclude(o => o.User)
                .Include(e => e.Categories)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(e => e.Status == status);

            if (!string.IsNullOrEmpty(city))
                query = query.Where(e => e.City.Contains(city));

            var events = await query.ToListAsync();

            return events.Select(e => MapToDto(e)).ToList();
        }

        public async Task<EventDto?> GetEventByIdAsync(int id)
        {
            var eventEntity = await _context.Events
                .Include(e => e.Organiser).ThenInclude(o => o.User)
                .Include(e => e.Categories)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (eventEntity == null) return null;

            return MapToDto(eventEntity);
        }

        public async Task<EventDto?> CreateEventAsync(int userId, CreateEventDto createDto)
        {
            // Get organiser
            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserID == userId);

            if (organiser == null) return null;

            var eventEntity = new Event
            {
                OrganiserID = organiser.OrganiserID,
                Title = createDto.Title,
                Description = createDto.Description,
                StartDate = createDto.StartDate,
                EndDate = createDto.EndDate,
                Location = createDto.Location,
                City = createDto.City,
                Province = createDto.Province,
                EventType = createDto.EventType,
                MaxParticipants = createDto.MaxParticipants,
                Status = "Draft",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Events.Add(eventEntity);
            await _context.SaveChangesAsync();

            return await GetEventByIdAsync(eventEntity.EventID);
        }

        public async Task<EventDto?> UpdateEventAsync(int eventId, int userId, UpdateEventDto updateDto)
        {
            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserID == userId);

            if (organiser == null) return null;

            var eventEntity = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == eventId && e.OrganiserID == organiser.OrganiserID);

            if (eventEntity == null) return null;

            eventEntity.Title = updateDto.Title;
            eventEntity.Description = updateDto.Description;
            eventEntity.StartDate = updateDto.StartDate;
            eventEntity.EndDate = updateDto.EndDate;
            eventEntity.Location = updateDto.Location;
            eventEntity.City = updateDto.City;
            eventEntity.Province = updateDto.Province;
            eventEntity.EventType = updateDto.EventType;
            eventEntity.MaxParticipants = updateDto.MaxParticipants;
            eventEntity.Status = updateDto.Status;
            eventEntity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetEventByIdAsync(eventEntity.EventID);
        }

        public async Task<bool> DeleteEventAsync(int eventId, int userId)
        {
            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserID == userId);

            if (organiser == null) return false;

            var eventEntity = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == eventId && e.OrganiserID == organiser.OrganiserID);

            if (eventEntity == null) return false;

            _context.Events.Remove(eventEntity);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<EventDto>> GetEventsByOrganiserAsync(int userId)
        {
            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserID == userId);

            if (organiser == null) return new List<EventDto>();

            var events = await _context.Events
                .Include(e => e.Organiser).ThenInclude(o => o.User)
                .Include(e => e.Categories)
                .Where(e => e.OrganiserID == organiser.OrganiserID)
                .ToListAsync();

            return events.Select(e => MapToDto(e)).ToList();
        }

        private EventDto MapToDto(Event e)
        {
            return new EventDto
            {
                EventID = e.EventID,
                Title = e.Title,
                Description = e.Description,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                Location = e.Location,
                City = e.City,
                Province = e.Province,
                EventType = e.EventType,
                MaxParticipants = e.MaxParticipants,
                Status = e.Status,
                OrganiserName = $"{e.Organiser?.User?.FirstName} {e.Organiser?.User?.LastName}",
                CategoryCount = e.Categories?.Count ?? 0
            };
        }
    }
}