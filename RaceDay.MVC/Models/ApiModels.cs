namespace RaceDay.MVC.Models
{
    public class LoginDto { public string Email { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
    public class RegisterDto { public string Email { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; public string FirstName { get; set; } = string.Empty; public string LastName { get; set; } = string.Empty; public string Role { get; set; } = string.Empty; }
    public class AuthResponseDto { public string Token { get; set; } = string.Empty; public string Role { get; set; } = string.Empty; public string FirstName { get; set; } = string.Empty; }

    public class EventDto
    {
        public int EventID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public int MaxParticipants { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class EnrolmentDto
    {
        public int EnrolmentID { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}