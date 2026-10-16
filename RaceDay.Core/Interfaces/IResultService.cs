using RaceDay.Core.DTOs;

namespace RaceDay.Core.Interfaces
{
    public interface IResultService
    {
        Task<ResultDto?> CreateResultAsync(int enrolmentId, int userId, CreateResultDto createDto);
        Task<ResultDto?> UpdateResultAsync(int resultId, int userId, CreateResultDto updateDto);
        Task<ResultDto?> GetResultByIdAsync(int resultId);
        Task<List<ResultDto>> GetResultsByParticipantAsync(int userId);
        Task<List<ResultDto>> GetResultsByEventAsync(int eventId);
    }
}