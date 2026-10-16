using RaceDay.Core.DTOs;

namespace RaceDay.Core.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetCategoriesByEventAsync(int eventId);
        Task<CategoryDto?> CreateCategoryAsync(int eventId, int userId, CreateCategoryDto createDto);
        Task<CategoryDto?> UpdateCategoryAsync(int categoryId, int userId, CreateCategoryDto updateDto);
        Task<bool> DeleteCategoryAsync(int categoryId, int userId);
    }
}