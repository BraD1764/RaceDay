using RaceDay.Core.DTOs;

namespace RaceDay.Core.Interfaces
{
    public interface IEnrolmentService
    {
        Task<EnrolmentDto?> CreateEnrolmentAsync(int userId, int categoryId);
        Task<List<EnrolmentDto>> GetEnrolmentsByParticipantAsync(int userId);
        Task<EnrolmentDto?> GetEnrolmentByIdAsync(int enrolmentId, int userId, string role);
        Task<List<EnrolmentDto>> GetEnrolmentsByEventAsync(int eventId, int userId);
        Task<bool> CancelEnrolmentAsync(int enrolmentId, int userId, string role);
    }
}