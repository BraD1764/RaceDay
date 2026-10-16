using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Core.Entities
{
    public class Enrolment
    {
        [Key]
        public int EnrolmentID { get; set; }

        [Required]
        public int ParticipantID { get; set; }

        [Required]
        public int CategoryID { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        [MaxLength(20)]
        public string? BibNumber { get; set; }

        [MaxLength(20)]
        public string PaymentStatus { get; set; } = "Unpaid";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ParticipantID")]
        public Participant Participant { get; set; } = null!;

        [ForeignKey("CategoryID")]
        public Category Category { get; set; } = null!;

        public Result? Result { get; set; }
    }
}