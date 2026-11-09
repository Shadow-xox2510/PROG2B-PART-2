
namespace RaceDayMVC.Models
{
    public class EnrolmentViewModel
    {
        public int EnrolmentID { get; set; }

        public int EventID { get; set; }

        public string EventName { get; set; } = "";

        public int ParticipantID { get; set; }

        public string ParticipantName { get; set; } = "";

        public string Email { get; set; } = "";

        public int CategoryID { get; set; }

        public DateTime EnrolmentDate { get; set; }

        public string Status { get; set; } = "";

        public bool HasResult { get; set; }
    }
}

