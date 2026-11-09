using System;

namespace RaceDayMVC.Models
{
    public class EventViewModel
    {
        public int EventID { get; set; }

        public string EventName { get; set; } = "";

        public string? Description { get; set; }

        public DateTime? EventDate { get; set; }

        public string? Location { get; set; }

        public double DistanceKm { get; set; }

        public string? EventType { get; set; }

        public int OrganiserID { get; set; }

        public string? Status { get; set; }

        public decimal EntryFee { get; set; }

        public int MaximumParticipants { get; set; }

        // ==========================================
        // EVENT CATEGORY INFORMATION
        // ==========================================

        public string CategoryName { get; set; } = "";

        public string CategoryDescription { get; set; } = "";
    }
}