using Microsoft.EntityFrameworkCore;
using RaceDay.Core.DTOs;
using RaceDay.Core.Entities;
using RaceDay.Core.Interfaces;
using RaceDay.Infrastructure.Data;

namespace RaceDay.Infrastructure.Services
{
    public class EnrolmentService : IEnrolmentService
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentService(RaceDayDbContext context)
        {
            _context = context;
        }

        public async Task<EnrolmentDto?> CreateEnrolmentAsync(int userId, int categoryId)
        {
            // Get participant
            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.UserID == userId);

            if (participant == null) return null;

            // Check if category exists and has space
            var category = await _context.Categories
                .Include(c => c.Enrolments)
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryID == categoryId);

            if (category == null) return null;

            if (category.Enrolments.Count >= category.MaxParticipants)
                return null;

            // Check if already enrolled
            var existing = await _context.Enrolments
                .FirstOrDefaultAsync(e => e.ParticipantID == participant.ParticipantID && e.CategoryID == categoryId);

            if (existing != null) return null;

            // Check event capacity
            var eventEnrolmentCount = await _context.Enrolments
                .Where(e => e.Category.EventID == category.EventID)
                .CountAsync();

            if (eventEnrolmentCount >= category.Event.MaxParticipants)
                return null;

            var enrolment = new Enrolment
            {
                ParticipantID = participant.ParticipantID,
                CategoryID = categoryId,
                RegistrationDate = DateTime.UtcNow,
                Status = "Confirmed",
                PaymentStatus = "Unpaid",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return await GetEnrolmentByIdAsync(enrolment.EnrolmentID, userId, "Participant");
        }

        public async Task<List<EnrolmentDto>> GetEnrolmentsByParticipantAsync(int userId)
        {
            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.UserID == userId);

            if (participant == null) return new List<EnrolmentDto>();

            var enrolments = await _context.Enrolments
                .Include(e => e.Participant).ThenInclude(p => p.User)
                .Include(e => e.Category).ThenInclude(c => c.Event)
                .Where(e => e.ParticipantID == participant.ParticipantID)
                .ToListAsync();

            return enrolments.Select(MapToDto).ToList();
        }

        public async Task<EnrolmentDto?> GetEnrolmentByIdAsync(int enrolmentId, int userId, string role)
        {
            var enrolment = await _context.Enrolments
                .Include(e => e.Participant).ThenInclude(p => p.User)
                .Include(e => e.Category).ThenInclude(c => c.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentID == enrolmentId);

            if (enrolment == null) return null;

            // Check permissions
            if (role == "Participant")
            {
                var participant = await _context.Participants
                    .FirstOrDefaultAsync(p => p.UserID == userId);
                if (participant == null || enrolment.ParticipantID != participant.ParticipantID)
                    return null;
            }
            else if (role == "Organiser")
            {
                var organiser = await _context.Organisers
                    .FirstOrDefaultAsync(o => o.UserID == userId);
                if (organiser == null || enrolment.Category.Event.OrganiserID != organiser.OrganiserID)
                    return null;
            }

            return MapToDto(enrolment);
        }

        public async Task<List<EnrolmentDto>> GetEnrolmentsByEventAsync(int eventId, int userId)
        {
            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserID == userId);

            if (organiser == null) return new List<EnrolmentDto>();

            var eventEntity = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == eventId && e.OrganiserID == organiser.OrganiserID);

            if (eventEntity == null) return new List<EnrolmentDto>();

            var enrolments = await _context.Enrolments
                .Include(e => e.Participant).ThenInclude(p => p.User)
                .Include(e => e.Category).ThenInclude(c => c.Event)
                .Where(e => e.Category.EventID == eventId)
                .ToListAsync();

            return enrolments.Select(MapToDto).ToList();
        }

        public async Task<bool> CancelEnrolmentAsync(int enrolmentId, int userId, string role)
        {
            var enrolment = await _context.Enrolments
                .Include(e => e.Category).ThenInclude(c => c.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentID == enrolmentId);

            if (enrolment == null) return false;

            // Check permissions
            if (role == "Participant")
            {
                var participant = await _context.Participants
                    .FirstOrDefaultAsync(p => p.UserID == userId);
                if (participant == null || enrolment.ParticipantID != participant.ParticipantID)
                    return false;
            }
            else if (role == "Organiser")
            {
                var organiser = await _context.Organisers
                    .FirstOrDefaultAsync(o => o.UserID == userId);
                if (organiser == null || enrolment.Category.Event.OrganiserID != organiser.OrganiserID)
                    return false;
            }

            enrolment.Status = "Cancelled";
            enrolment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        private EnrolmentDto MapToDto(Enrolment e)
        {
            return new EnrolmentDto
            {
                EnrolmentID = e.EnrolmentID,
                ParticipantID = e.ParticipantID,
                ParticipantName = $"{e.Participant?.User?.FirstName} {e.Participant?.User?.LastName}",
                CategoryID = e.CategoryID,
                CategoryName = e.Category?.CategoryName ?? string.Empty,
                EventTitle = e.Category?.Event?.Title ?? string.Empty,
                RegistrationDate = e.RegistrationDate,
                Status = e.Status,
                BibNumber = e.BibNumber,
                PaymentStatus = e.PaymentStatus
            };
        }
    }
}