using System.Collections.Generic;

namespace RaceDayMVC.Models
{
    public class ParticipantEventViewModel
    {
        public EventViewModel Event { get; set; } = new();

        public List<CategoryViewModel> Categories { get; set; } = new();

        public int SelectedCategoryID { get; set; }
    }
}