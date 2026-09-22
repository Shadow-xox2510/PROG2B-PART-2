namespace RaceDayAPI.Models
{
    public class EventEnrolment
    {
        public int EnrolmentID { get; set; }

        public int EventID { get; set; }

        public int UserID { get; set; }

        public int CategoryID { get; set; }

        public DateTime EnrolmentDate { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
