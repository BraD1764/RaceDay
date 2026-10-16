using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RaceDay.Core.DTOs;
using RaceDay.Core.Interfaces;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class ResultsController : ControllerBase
    {
        private readonly IResultService _resultService;

        public ResultsController(IResultService resultService)
        {
            _resultService = resultService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : 0;
        }

        /// <summary>
        /// Capture result for an enrolment (Organiser only)
        /// </summary>
        [HttpPost("enrolments/{enrolmentId}/result")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateResult(int enrolmentId, [FromBody] CreateResultDto createDto)
        {
            var userId = GetCurrentUserId();
            var result = await _resultService.CreateResultAsync(enrolmentId, userId, createDto);

            if (result == null)
                return NotFound(new { message = "Enrolment not found or you don't have permission" });

            return CreatedAtAction(nameof(GetResultById), new { id = result.ResultID }, result);
        }

        /// <summary>
        /// Update a result (Organiser only)
        /// </summary>
        [HttpPut("results/{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateResult(int id, [FromBody] CreateResultDto updateDto)
        {
            var userId = GetCurrentUserId();
            var result = await _resultService.UpdateResultAsync(id, userId, updateDto);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        /// <summary>
        /// Get result by ID
        /// </summary>
        [HttpGet("results/{id}")]
        [Authorize]
        public async Task<IActionResult> GetResultById(int id)
        {
            var result = await _resultService.GetResultByIdAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        /// <summary>
        /// Get current participant's results
        /// </summary>
        [HttpGet("results/my-results")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyResults()
        {
            var userId = GetCurrentUserId();
            var results = await _resultService.GetResultsByParticipantAsync(userId);
            return Ok(results);
        }

        /// <summary>
        /// Get all results for an event (public)
        /// </summary>
        [HttpGet("events/{eventId}/results")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEventResults(int eventId)
        {
            var results = await _resultService.GetResultsByEventAsync(eventId);
            return Ok(results);
        }
    }
}