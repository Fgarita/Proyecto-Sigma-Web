using Carwash.Models;
using Carwash.Models.ViewModel;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;

namespace Carwash.Controllers
{
    public class ContactController : Controller
    {
        public ActionResult Contact_Form()
        {
            return View();
        }

        public ActionResult Contact_Confirmation()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Contact(ContactViewModel model)
        {

            if (ModelState.IsValid)
            {
                //using (var db = new carwashEntities())
                //{
                //    db.notifications.Add(new notifications
                //    {
                //        Nombre = model.Name,
                //        CorreoUsuario = model.email,
                //        Mensaje = model.message
                //    });

                    //db.SaveChanges();
                    Send_Email_User(model.Email, model.Name);
                    Send_Email_Company(model.Name, model.Email, model.Phone, model.Subject, model.Message);
                    return RedirectToAction("Contact_Confirmation");
                }
                return View("Contact_Form", model);
            }

        private void Send_Email_User(string correoUsuario, string nombre)
        {
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress("vettelcarwash@gmail.com");
                mail.To.Add(correoUsuario);
                mail.Subject = "Gracias por contactarnos";

                // Código HTML personalizado para el cuerpo del correo
                mail.Body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <title>Correo de Ejemplo</title>
</head>
<body>
    <div style=""font-family: Arial, sans-serif;"">
        <div style=""background-color: #f8f8f8; padding: 20px;"">
        </div>
        <div style=""padding: 20px;"">
            <p>Hola {nombre},</p>
            <p>Gracias por contactarnos. Hemos recibido tu mensaje y nos pondremos en contacto contigo pronto.</p>
            <br>
            <p>Esperamos que tengas un excelente día.</p>
            <br>
            <p>¡Gracias por elegir nuestros servicios!</p>
        </div>
    </div>
</body>
</html>
        ";
                mail.IsBodyHtml = true;



                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential("vettelcarwash@gmail.com", "eyvmzrwsuhxwruec");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
        }

        private void Send_Email_Company(string nombre, string correoUsuario, string telefonoUsuario, string subject, string mensaje)
        {
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(correoUsuario);
                mail.To.Add("vettelcarwash@gmail.com");
                mail.Subject = $"Mensaje de contacto de usuario {nombre}";
                mail.Body = $"Nombre: {nombre}<br/>Correo: {correoUsuario}<br/>Telefono: {telefonoUsuario} <br/>Asunto: {subject}  <br/>Mensaje: {mensaje} ";
                mail.IsBodyHtml = true;

                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential("vettelcarwash@gmail.com", "eyvmzrwsuhxwruec");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
        }

    }
}