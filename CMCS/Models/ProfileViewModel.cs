using System.ComponentModel.DataAnnotations;

namespace CMCS.Models
{
    public class ProfileViewModel
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; } // Optional, user doesn't have to change it

        public IFormFile ProfilePicture { get; set; }
    }
}
