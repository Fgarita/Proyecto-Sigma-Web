using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Carwash.Models.ViewModel
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "El nombre debe contener solo letras y espacios.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Por favor, introduce una dirección de correo válida.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El número de teléfono debe tener 8 dígitos.")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "El Asunto es obligatorio.")]
        public string Subject { get; set; }
        [Required(ErrorMessage = "El Mensaje es obligatorio.")]
        public string Message { get; set; }
    }
}