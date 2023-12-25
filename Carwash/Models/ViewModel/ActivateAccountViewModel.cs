using System.ComponentModel.DataAnnotations;

namespace Carwash.Models.ViewModel
{
    public class ActivateAccountViewModel
    {
        public string Token { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}