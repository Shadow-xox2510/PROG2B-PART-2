using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDayMVC.Models
{
    public class ProfileViewModel
    {
        public int UserID { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "Last name is required.")]
        public string LastName { get; set; } = "";

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = "";

        public string Role { get; set; } = "";

        [Required(ErrorMessage = "Phone number is required.")]
        public string PhoneNumber { get; set; } = "";

        public DateTime DateRegistered { get; set; }
    }
}