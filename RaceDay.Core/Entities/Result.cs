using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Core.Entities
{
    public class Result
    {
        [Key]
        public int ResultID { get; set; }

        [Required]
        public int EnrolmentID { get; set; }

        public TimeSpan? FinishTime { get; set; }
        public int? OverallRank { get; set; }
        public int? AgeGroupRank { get; set; }
        public int? GenderRank { get; set; }
        public TimeSpan? ChipTime { get; set; }
        public TimeSpan? GunTime { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("EnrolmentID")]
        public Enrolment Enrolment { get; set; } = null!;
    }
}