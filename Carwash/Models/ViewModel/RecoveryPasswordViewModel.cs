using System.ComponentModel.DataAnnotations;

namespace Carwash.Models.ViewModel
{
    public class RecoveryPasswordViewModel
    {
        public string token { get; set; }

        [Required]
        public string Password { get; set; }

        [Compare("Password")]
        [Required]
        public string ConfirmPassword { get; set; }
    }
}