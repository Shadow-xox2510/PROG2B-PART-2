namespace RaceDayAPI.DTOs
{
    public class ResultDto
    {
        public int EnrolmentID { get; set; }
        public TimeSpan FinishTime { get; set; }
        public int Position { get; set; }
        public string ResultStatus { get; set; } = string.Empty;
    }
}
