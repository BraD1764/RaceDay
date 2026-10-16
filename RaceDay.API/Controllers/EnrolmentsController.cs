using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RaceDay.Core.DTOs;
using RaceDay.Core.Interfaces;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class EnrolmentsController : ControllerBase
    {
        private readonly IEnrolmentService _enrolmentService;

        public EnrolmentsController(IEnrolmentService enrolmentService)
        {
            _enrolmentService = enrolmentService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : 0;
        }

        /// <summary>
        /// Enrol in a category (Participant only)
        /// </summary>
        [HttpPost("categories/{categoryId}/enrolments")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> Enrol(int categoryId)
        {
            var userId = GetCurrentUserId();
            var enrolment = await _enrolmentService.CreateEnrolmentAsync(userId, categoryId);

            if (enrolment == null)
                return BadRequest(new { message = "Could not enrol. Category may be full or you may already be enrolled" });

            return CreatedAtAction(nameof(GetEnrolmentById), new { id = enrolment.EnrolmentID }, enrolment);
        }

        /// <summary>
        /// Get current participant's enrolments
        /// </summary>
        [HttpGet("enrolments/my-enrolments")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var userId = GetCurrentUserId();
            var enrolments = await _enrolmentService.GetEnrolmentsByParticipantAsync(userId);
            return Ok(enrolments);
        }

        /// <summary>
        /// Get enrolment by ID
        /// </summary>
        [HttpGet("enrolments/{id}")]
        public async Task<IActionResult> GetEnrolmentById(int id)
        {
            var userId = GetCurrentUserId();
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

            var enrolment = await _enrolmentService.GetEnrolmentByIdAsync(id, userId, role);

            if (enrolment == null)
                return NotFound();

            return Ok(enrolment);
        }

        /// <summary>
        /// Get all enrolments for an event (Organiser only)
        /// </summary>
        [HttpGet("events/{eventId}/enrolments")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetEventEnrolments(int eventId)
        {
            var userId = GetCurrentUserId();
            var enrolments = await _enrolmentService.GetEnrolmentsByEventAsync(eventId, userId);
            return Ok(enrolments);
        }

        /// <summary>
        /// Cancel an enrolment
        /// </summary>
        [HttpDelete("enrolments/{id}")]
        public async Task<IActionResult> CancelEnrolment(int id)
        {
            var userId = GetCurrentUserId();
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

            var result = await _enrolmentService.CancelEnrolmentAsync(id, userId, role);

            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}