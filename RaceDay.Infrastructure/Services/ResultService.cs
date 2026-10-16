using Microsoft.EntityFrameworkCore;
using RaceDay.Core.DTOs;
using RaceDay.Core.Entities;
using RaceDay.Core.Interfaces;
using RaceDay.Infrastructure.Data;

namespace RaceDay.Infrastructure.Services
{
    public class ResultService : IResultService
    {
        private readonly RaceDayDbContext _context;

        public ResultService(RaceDayDbContext context)
        {
            _context = context;
        }

        public async Task<ResultDto?> CreateResultAsync(int enrolmentId, int userId, CreateResultDto createDto)
        {
            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserID == userId);

            if (organiser == null) return null;

            var enrolment = await _context.Enrolments
                .Include(e => e.Category).ThenInclude(c => c.Event)
                .Include(e => e.Participant).ThenInclude(p => p.User)
                .FirstOrDefaultAsync(e => e.EnrolmentID == enrolmentId);

            if (enrolment == null || enrolment.Category.Event.OrganiserID != organiser.OrganiserID)
                return null;

            // Check if result already exists
            var existingResult = await _context.Results
                .FirstOrDefaultAsync(r => r.EnrolmentID == enrolmentId);

            if (existingResult != null)
                return null;

            var result = new Result
            {
                EnrolmentID = enrolmentId,
                FinishTime = createDto.FinishTime,
                ChipTime = createDto.ChipTime,
                GunTime = createDto.GunTime,
                Status = createDto.Status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            // Calculate rankings
            await CalculateRankings(enrolment.CategoryID);

            return await GetResultByIdAsync(result.ResultID);
        }

        public async Task<ResultDto?> UpdateResultAsync(int resultId, int userId, CreateResultDto updateDto)
        {
            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserID == userId);

            if (organiser == null) return null;

            var result = await _context.Results
                .Include(r => r.Enrolment).ThenInclude(e => e.Category).ThenInclude(c => c.Event)
                .FirstOrDefaultAsync(r => r.ResultID == resultId);

            if (result == null || result.Enrolment.Category.Event.OrganiserID != organiser.OrganiserID)
                return null;

            result.FinishTime = updateDto.FinishTime;
            result.ChipTime = updateDto.ChipTime;
            result.GunTime = updateDto.GunTime;
            result.Status = updateDto.Status;
            result.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await CalculateRankings(result.Enrolment.CategoryID);

            return await GetResultByIdAsync(resultId);
        }

        public async Task<ResultDto?> GetResultByIdAsync(int resultId)
        {
            var result = await _context.Results
                .Include(r => r.Enrolment).ThenInclude(e => e.Participant).ThenInclude(p => p.User)
                .Include(r => r.Enrolment).ThenInclude(e => e.Category).ThenInclude(c => c.Event)
                .FirstOrDefaultAsync(r => r.ResultID == resultId);

            if (result == null) return null;

            return MapToDto(result);
        }

        public async Task<List<ResultDto>> GetResultsByParticipantAsync(int userId)
        {
            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.UserID == userId);

            if (participant == null) return new List<ResultDto>();

            var results = await _context.Results
                .Include(r => r.Enrolment).ThenInclude(e => e.Participant).ThenInclude(p => p.User)
                .Include(r => r.Enrolment).ThenInclude(e => e.Category).ThenInclude(c => c.Event)
                .Where(r => r.Enrolment.ParticipantID == participant.ParticipantID)
                .ToListAsync();

            return results.Select(MapToDto).ToList();
        }

        public async Task<List<ResultDto>> GetResultsByEventAsync(int eventId)
        {
            var results = await _context.Results
                .Include(r => r.Enrolment).ThenInclude(e => e.Participant).ThenInclude(p => p.User)
                .Include(r => r.Enrolment).ThenInclude(e => e.Category).ThenInclude(c => c.Event)
                .Where(r => r.Enrolment.Category.EventID == eventId)
                .OrderBy(r => r.OverallRank)
                .ToListAsync();

            return results.Select(MapToDto).ToList();
        }

        private async Task CalculateRankings(int categoryId)
        {
            var results = await _context.Results
                .Include(r => r.Enrolment)
                .Where(r => r.Enrolment.CategoryID == categoryId && r.Status == "Finished" && r.FinishTime != null)
                .OrderBy(r => r.FinishTime)
                .ToListAsync();

            int rank = 1;
            foreach (var result in results)
            {
                result.OverallRank = rank++;
            }

            await _context.SaveChangesAsync();
        }

        private ResultDto MapToDto(Result r)
        {
            return new ResultDto
            {
                ResultID = r.ResultID,
                EnrolmentID = r.EnrolmentID,
                ParticipantName = $"{r.Enrolment?.Participant?.User?.FirstName} {r.Enrolment?.Participant?.User?.LastName}",
                EventTitle = r.Enrolment?.Category?.Event?.Title ?? string.Empty,
                CategoryName = r.Enrolment?.Category?.CategoryName ?? string.Empty,
                FinishTime = r.FinishTime,
                OverallRank = r.OverallRank,
                AgeGroupRank = r.AgeGroupRank,
                GenderRank = r.GenderRank,
                ChipTime = r.ChipTime,
                GunTime = r.GunTime,
                Status = r.Status
            };
        }
    }
}