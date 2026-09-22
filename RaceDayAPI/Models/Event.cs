namespace RaceDayAPI.Models
{
    public class Event
    {
        public int EventID { get; set; }

        public int OrganiserID { get; set; }

        public int RouteID { get; set; }

        public string EventName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }

        public string Location { get; set; } = string.Empty;

        public decimal EntryFee { get; set; }

        public int MaximumParticipants { get; set; }

        public string Status { get; set; } = string.Empty;

        public string EventType { get; set; } = string.Empty;
    }
}
