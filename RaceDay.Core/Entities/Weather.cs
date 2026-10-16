using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Core.Entities
{
    public class Weather
    {
        [Key]
        public int WeatherID { get; set; }

        [Required]
        public int EventID { get; set; }

        [Required]
        public DateTime ForecastDate { get; set; }

        public decimal? Temperature { get; set; }

        [MaxLength(100)]
        public string? Condition { get; set; }

        public decimal? Humidity { get; set; }
        public decimal? WindSpeed { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("EventID")]
        public Event Event { get; set; } = null!;
    }
}