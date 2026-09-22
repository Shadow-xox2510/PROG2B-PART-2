namespace RaceDayAPI.Models
{
    public class Route
    {
        public int RouteID { get; set; }

        public string RouteName { get; set; } = string.Empty;

        public decimal DistanceKm { get; set; }

        public string StartLocation { get; set; } = string.Empty;

        public string FinishLocation { get; set; } = string.Empty;
    }
}
