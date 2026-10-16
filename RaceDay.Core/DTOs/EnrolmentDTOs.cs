namespace RaceDay.Core.DTOs
{
    public class EnrolmentDto
    {
        public int EnrolmentID { get; set; }
        public int ParticipantID { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string EventTitle { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? BibNumber { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
    }

    public class CreateEnrolmentDto
    {
        public int CategoryID { get; set; }
    }
}