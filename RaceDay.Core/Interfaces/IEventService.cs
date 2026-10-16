using RaceDay.Core.DTOs;

namespace RaceDay.Core.Interfaces
{
    public interface IEventService
    {
        Task<List<EventDto>> GetAllEventsAsync(string? status, string? city);
        Task<EventDto?> GetEventByIdAsync(int id);
        Task<EventDto?> CreateEventAsync(int userId, CreateEventDto createDto);
        Task<EventDto?> UpdateEventAsync(int eventId, int userId, UpdateEventDto updateDto);
        Task<bool> DeleteEventAsync(int eventId, int userId);
        Task<List<EventDto>> GetEventsByOrganiserAsync(int userId);
    }
}