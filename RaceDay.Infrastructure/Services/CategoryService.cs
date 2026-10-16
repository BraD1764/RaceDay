using Microsoft.EntityFrameworkCore;
using RaceDay.Core.DTOs;
using RaceDay.Core.Entities;
using RaceDay.Core.Interfaces;
using RaceDay.Infrastructure.Data;

namespace RaceDay.Infrastructure.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly RaceDayDbContext _context;

        public CategoryService(RaceDayDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryDto>> GetCategoriesByEventAsync(int eventId)
        {
            var categories = await _context.Categories
                .Include(c => c.Enrolments)
                .Where(c => c.EventID == eventId)
                .ToListAsync();

            return categories.Select(c => new CategoryDto
            {
                CategoryID = c.CategoryID,
                EventID = c.EventID,
                CategoryName = c.CategoryName,
                Distance = c.Distance,
                EntryFee = c.EntryFee,
                MaxParticipants = c.MaxParticipants,
                CurrentParticipants = c.Enrolments?.Count ?? 0
            }).ToList();
        }

        public async Task<CategoryDto?> CreateCategoryAsync(int eventId, int userId, CreateCategoryDto createDto)
        {
            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserID == userId);

            if (organiser == null) return null;

            var eventEntity = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == eventId && e.OrganiserID == organiser.OrganiserID);

            if (eventEntity == null) return null;

            var category = new Category
            {
                EventID = eventId,
                CategoryName = createDto.CategoryName,
                Distance = createDto.Distance,
                EntryFee = createDto.EntryFee,
                MaxParticipants = createDto.MaxParticipants,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return new CategoryDto
            {
                CategoryID = category.CategoryID,
                EventID = category.EventID,
                CategoryName = category.CategoryName,
                Distance = category.Distance,
                EntryFee = category.EntryFee,
                MaxParticipants = category.MaxParticipants,
                CurrentParticipants = 0
            };
        }

        public async Task<CategoryDto?> UpdateCategoryAsync(int categoryId, int userId, CreateCategoryDto updateDto)
        {
            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserID == userId);

            if (organiser == null) return null;

            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryID == categoryId && c.Event.OrganiserID == organiser.OrganiserID);

            if (category == null) return null;

            category.CategoryName = updateDto.CategoryName;
            category.Distance = updateDto.Distance;
            category.EntryFee = updateDto.EntryFee;
            category.MaxParticipants = updateDto.MaxParticipants;
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new CategoryDto
            {
                CategoryID = category.CategoryID,
                EventID = category.EventID,
                CategoryName = category.CategoryName,
                Distance = category.Distance,
                EntryFee = category.EntryFee,
                MaxParticipants = category.MaxParticipants
            };
        }

        public async Task<bool> DeleteCategoryAsync(int categoryId, int userId)
        {
            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserID == userId);

            if (organiser == null) return false;

            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryID == categoryId && c.Event.OrganiserID == organiser.OrganiserID);

            if (category == null) return false;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}