using System.ComponentModel.DataAnnotations;

namespace Carwash.Models.ViewModel
{
    public class RecoveryViewModel
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }

    }
}