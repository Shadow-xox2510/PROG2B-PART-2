using System.ComponentModel.DataAnnotations;

namespace RaceDayMVC.Models
{
    public class CaptureResultViewModel
    {
        [Range(1, int.MaxValue,
            ErrorMessage = "A valid enrolment ID is required.")]
        public int EnrolmentID { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "A valid event ID is required.")]
        public int EventID { get; set; }

        public string ParticipantName { get; set; } = "";

        [Required(ErrorMessage = "Finish time is required.")]
        public string FinishTime { get; set; } = "00:00:00";

        [Range(1, int.MaxValue,
            ErrorMessage = "Finishing position must be greater than zero.")]
        public int Position { get; set; }

        [Required(ErrorMessage = "Result status is required.")]
        [RegularExpression("^(Published|Pending)$",
            ErrorMessage = "Select Published or Pending.")]
        public string ResultStatus { get; set; } = "Published";
    }
}