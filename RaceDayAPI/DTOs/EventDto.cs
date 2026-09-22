namespace RaceDayAPI.DTOs
{
    public class EventDto
    {
        public string EventName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryDescription { get; set; } = string.Empty;
        public decimal EntryFee { get; set; }
        public int MaximumParticipants { get; set; }
    }
}
