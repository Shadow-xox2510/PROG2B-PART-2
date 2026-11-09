

namespace RaceDayMVC.Models
{
    public class ResultViewModel
    {
        public int ResultID { get; set; }

        public int EnrolmentID { get; set; }

        public int EventID { get; set; }

        public string EventName { get; set; } = "";

        public TimeSpan FinishTime { get; set; }

        public int Position { get; set; }

        public string ResultStatus { get; set; } = "";
    }
}
