
using System.ComponentModel.DataAnnotations;

namespace RaceDayMVC.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "First name is required.")]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "Last name is required.")]
        public string LastName { get; set; } = "";

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Please select a role.")]
        [RegularExpression("^(Organiser|Participant)$",
            ErrorMessage = "Select Organiser or Participant.")]
        public string Role { get; set; } = "Participant";

        [Required(ErrorMessage = "Phone number is required.")]
        public string PhoneNumber { get; set; } = "";
    }
}

