using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RaceDay.Core.DTOs;
using RaceDay.Core.Interfaces;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : 0;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        /// <summary>
        /// Get all events (public)
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllEvents([FromQuery] string? status, [FromQuery] string? city)
        {
            var events = await _eventService.GetAllEventsAsync(status, city);
            return Ok(events);
        }

        /// <summary>
        /// Get event by ID (public)
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEventById(int id)
        {
            var eventDto = await _eventService.GetEventByIdAsync(id);

            if (eventDto == null)
                return NotFound(new { message = "Event not found" });

            return Ok(eventDto);
        }

        /// <summary>
        /// Create a new event (Organiser only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var eventDto = await _eventService.CreateEventAsync(userId, createDto);

            if (eventDto == null)
                return BadRequest(new { message = "Could not create event" });

            return CreatedAtAction(nameof(GetEventById), new { id = eventDto.EventID }, eventDto);
        }

        /// <summary>
        /// Update an event (Organiser only, must own the event)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var eventDto = await _eventService.UpdateEventAsync(id, userId, updateDto);

            if (eventDto == null)
                return NotFound(new { message = "Event not found or you don't have permission" });

            return Ok(eventDto);
        }

        /// <summary>
        /// Delete an event (Organiser only, must own the event)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userId = GetCurrentUserId();
            var result = await _eventService.DeleteEventAsync(id, userId);

            if (!result)
                return NotFound(new { message = "Event not found or you don't have permission" });

            return NoContent();
        }

        /// <summary>
        /// Get events for the current organiser
        /// </summary>
        [HttpGet("my-events")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetMyEvents()
        {
            var userId = GetCurrentUserId();
            var events = await _eventService.GetEventsByOrganiserAsync(userId);
            return Ok(events);
        }
    }
}