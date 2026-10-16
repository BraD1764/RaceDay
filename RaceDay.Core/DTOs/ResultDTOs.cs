namespace RaceDay.Core.DTOs
{
    public class ResultDto
    {
        public int ResultID { get; set; }
        public int EnrolmentID { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string EventTitle { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public TimeSpan? FinishTime { get; set; }
        public int? OverallRank { get; set; }
        public int? AgeGroupRank { get; set; }
        public int? GenderRank { get; set; }
        public TimeSpan? ChipTime { get; set; }
        public TimeSpan? GunTime { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class CreateResultDto
    {
        public TimeSpan? FinishTime { get; set; }
        public TimeSpan? ChipTime { get; set; }
        public TimeSpan? GunTime { get; set; }
        public string Status { get; set; } = "Finished";
    }
}