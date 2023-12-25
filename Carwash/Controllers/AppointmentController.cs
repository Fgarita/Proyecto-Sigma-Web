using AppWebWithSweetAlert.Controllers;
using Carwash.Models;
using Carwash.Models.ViewModel;
using DocumentFormat.OpenXml.EMMA;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static Carwash.Models.Enum;

namespace Carwash.Controllers
{
    public class AppointmentController : BaseController
    {

        carwashEntities db = new carwashEntities();
        string urlDomain = "https://localhost:44325/";

        public ActionResult UsersAppointments()
        {
            List<appointments> allAppointments = db.appointments.ToList();
            List<AppointmentUserViewModel> appointmentList = new List<AppointmentUserViewModel>();

            foreach (var appointments in allAppointments)
            {
                var user = db.users.FirstOrDefault(u => u.user_id == appointments.user_id);
                var service = db.service.FirstOrDefault(s => s.service_id == appointments.service_id);

                if (user != null)
                {
                    AppointmentUserViewModel appointmentUser = new AppointmentUserViewModel
                    {
                        appointment_id = appointments.appointment_id,
                        date = appointments.date,
                        email = user.email,
                        phone = user.phone,
                        time = appointments.time,
                        site = appointments.site,
                        name = user.name,
                        service = service.name
                    };
                    appointmentList.Add(appointmentUser);
                }
            }

            return View(appointmentList);
        }

        public ActionResult Index()
        {
            string userEmail = Session["Email"] as string;
            var user = db.users.FirstOrDefault(u => u.email == userEmail);

            if (user != null)
            {
                int userId = user.user_id;

                // Filtrar las citas asociadas al usuario logueado
                List<appointments> userAppointments = db.appointments.Where(a => a.user_id == userId).ToList();

                return View(userAppointments);
            }
            return RedirectToAction("Index", "User");
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            appointments appointment = db.appointments.SingleOrDefault(u => u.appointment_id == id);

            if (appointment == null)
            {
                return HttpNotFound();
            }

            ViewBag.AppointmentId = appointment.appointment_id;
            // Obtén la propiedad 'time' de la cita
            TimeSpan appointmentTime = appointment.time;

            // Formatea el tiempo como cadena en formato "HH:mm"
            string appointmentTimeFormatted = appointmentTime.ToString(@"hh\:mm");

            // Obtén todas las opciones de tiempo
            IEnumerable<SelectListItem> allTimeOptions = GetTimeOptions();
            // Agrega la opción seleccionada al conjunto de opciones
            List<SelectListItem> timeOptions = allTimeOptions.ToList();
            timeOptions.Add(new SelectListItem { Value = appointmentTimeFormatted, Text = appointmentTimeFormatted });

            ViewBag.AllTimeOptions = timeOptions;
            ViewBag.SelectedTime = appointmentTimeFormatted; // Nueva clave para el valor seleccionado
            //ViewBag.TimeOptions = GetTimeOptions();
            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(appointments appointment)
        {
            if (ModelState.IsValid)
            {
                string userEmail = Session["Email"] as string;
                var user = db.users.FirstOrDefault(u => u.email == userEmail);

                if (user != null)
                {
                    if (appointment.service_id == 0)
                    {
                        ModelState.AddModelError("service_id", "Debe seleccionar un servicio.");
                        ViewBag.TimeOptions = GetTimeOptions();
                        return View(appointment);
                    }

                    service Services = db.service.SingleOrDefault(u => u.service_id == appointment.service_id);
                    appointment.name = Services.name;
                    appointment.user_id = user.user_id;
                    db.appointments.Attach(appointment);
                    db.Entry(appointment).State = EntityState.Modified;
                    db.SaveChanges();
                    Alert("Cita Actualizada", NotificationTypeMessage.Éxito, NotificationType.success);
                    SendEmail(userEmail);
                    Send_Email_Company(user.name, userEmail, "He agendado una cita para el lavado de mi vehiculo.");

                    if (Session["Rol"].Equals("administrator") || Session["Rol"].Equals("user"))
                    {
                        return RedirectToAction("UsersAppointments");
                    }
                    else
                    {
                        return RedirectToAction("Index");
                    }

                }
                else
                {
                    ViewBag.TimeOptions = GetTimeOptions();
                    return View();
                }
            }
            ViewBag.TimeOptions = GetTimeOptions();
            return View(appointment);
        }

        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.TimeOptions = GetTimeOptions();
            return View();
        }

        [HttpPost]
        public ActionResult Create(appointments appointment)
        {
            if (ModelState.IsValid || true)
            {
                string userEmail = Session["Email"] as string;
                var user = db.users.FirstOrDefault(u => u.email == userEmail);
                DateTime currentDate = DateTime.Now;

                if (user != null)
                {
                    if (appointment.service_id == 0)
                    {
                        ModelState.AddModelError("service_id", "Debe seleccionar un servicio.");
                        ViewBag.TimeOptions = GetTimeOptions();
                        return View(appointment);
                    }

                    if (appointment.site == "0")
                    {
                        ModelState.AddModelError("site", "Debe seleccionar una sede.");
                        ViewBag.TimeOptions = GetTimeOptions();
                        return View(appointment);
                    }

                    if (appointment.date == null)
                    {
                        ModelState.AddModelError("site", "Debe seleccionar una fecha.");
                        ViewBag.TimeOptions = GetTimeOptions();
                        return View(appointment);
                    }

                    if (appointment.date < currentDate)
                    {
                        ModelState.AddModelError("date", "No puedes seleccionar una fecha anterior a la fecha actual.");
                        ViewBag.TimeOptions = GetTimeOptions();
                        return View(appointment);
                    }
                    service Services = db.service.SingleOrDefault(u => u.service_id == appointment.service_id);
                    appointment.name = Services.name;

                    appointment.user_id = user.user_id;
                    db.appointments.Add(appointment);
                    db.SaveChanges();
                    Alert("Cita agendada", NotificationTypeMessage.Éxito, NotificationType.success);
                    SendEmail(userEmail);
                    Send_Email_Company(user.name, userEmail, "He agendado una cita para el lavado de mi vehiculo.");
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.TimeOptions = GetTimeOptions();
                    return View();
                }
            }

            return View(appointment);

        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {

                appointments appointment = db.appointments.Find(id);

                if (appointment != null)
                {

                    db.appointments.Remove(appointment);
                    db.SaveChanges();
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        private SelectList GetTimeOptions()
        {
            List<SelectListItem> timeOptions = new List<SelectListItem>();

            for (int hour = 7; hour <= 17; hour++)
            {
                for (int minute = 0; minute < 60; minute += 30)
                {
                    string displayValue = $"{hour:00}:{minute:00}";
                    timeOptions.Add(new SelectListItem { Text = displayValue, Value = displayValue });
                }
            }

            return new SelectList(timeOptions, "Value", "Text");
        }

        #region Helpers

        private void SendEmail(string toAddress)
        {
            string fromAddress = "vettelcarwash@gmail.com";
            string Contrasena = "eyvmzrwsuhxwruec";
            string subject = "Cita agendada para su vehiculo";
            string body = "<p>Enhorabuena su cita ha sido agendada con nosotros. Lo esperamos pronto!</p><br>";

            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(fromAddress, Contrasena);

            MailMessage message = new MailMessage(fromAddress, toAddress, subject, body);
            message.IsBodyHtml = true;

            client.Send(message);

            client.Dispose();
        }

        private void Send_Email_Company(string nombre, string correoUsuario, string subject)
        {
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(correoUsuario);
                mail.To.Add("vettelcarwash@gmail.com");
                mail.Subject = $"Cita para usuario {nombre}";
                mail.Body = $"Correo: {correoUsuario} <br/>Asunto: {subject}";
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

        #endregion
    }
}