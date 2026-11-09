

using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDayMVC.Models
{
    public class CreateEventViewModel
    {
        [Required(ErrorMessage = "Event name is required.")]
        public string EventName { get; set; } = "";

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = "";

        [Required(ErrorMessage = "Event date is required.")]
        [DataType(DataType.Date)]
        public DateTime EventDate { get; set; } =
            DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Location is required.")]
        public string Location { get; set; } = "";

        [Required(ErrorMessage = "Select an event type.")]
        [RegularExpression("^(Run|Walk|Cycle)$",
            ErrorMessage = "Choose Run, Walk or Cycle.")]
        public string EventType { get; set; } = "Run";

        [Required(ErrorMessage = "Category name is required.")]
        public string CategoryName { get; set; } = "";

        public string CategoryDescription { get; set; } = "";

        [Range(0, 100000,
            ErrorMessage = "Entry fee cannot be negative.")]
        public decimal EntryFee { get; set; }

        [Range(1, 100000,
            ErrorMessage = "Maximum participants must be at least 1.")]
        public int MaximumParticipants { get; set; } = 100;

        [Range(0.1, 1000,
            ErrorMessage = "Distance must be greater than zero.")]
        public double DistanceKm { get; set; } = 5;
    }
}
