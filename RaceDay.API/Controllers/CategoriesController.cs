using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RaceDay.Core.DTOs;
using RaceDay.Core.Interfaces;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : 0;
        }

        /// <summary>
        /// Get all categories for an event (public)
        /// </summary>
        [HttpGet("events/{eventId}/categories")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategoriesByEvent(int eventId)
        {
            var categories = await _categoryService.GetCategoriesByEventAsync(eventId);
            return Ok(categories);
        }

        /// <summary>
        /// Create a category for an event (Organiser only)
        /// </summary>
        [HttpPost("events/{eventId}/categories")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateCategory(int eventId, [FromBody] CreateCategoryDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var category = await _categoryService.CreateCategoryAsync(eventId, userId, createDto);

            if (category == null)
                return NotFound(new { message = "Event not found or you don't have permission" });

            return CreatedAtAction(nameof(GetCategoriesByEvent), new { eventId }, category);
        }

        /// <summary>
        /// Update a category (Organiser only)
        /// </summary>
        [HttpPut("categories/{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CreateCategoryDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var category = await _categoryService.UpdateCategoryAsync(id, userId, updateDto);

            if (category == null)
                return NotFound(new { message = "Category not found or you don't have permission" });

            return Ok(category);
        }

        /// <summary>
        /// Delete a category (Organiser only)
        /// </summary>
        [HttpDelete("categories/{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var userId = GetCurrentUserId();
            var result = await _categoryService.DeleteCategoryAsync(id, userId);

            if (!result)
                return NotFound(new { message = "Category not found or you don't have permission" });

            return NoContent();
        }
    }
}