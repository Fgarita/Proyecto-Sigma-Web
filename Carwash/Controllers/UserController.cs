using AppWebWithSweetAlert.Controllers;
using Carwash.Models;
using Carwash.Models.ViewModel;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using static Carwash.Models.Enum;

namespace Carwash.Controllers
{
    public class UserController : BaseController
    {
        //private static string db = ConfigurationManager.ConnectionStrings["cadena"].ToString();
        carwashEntities db = new carwashEntities();
        //string urlDomain = "https://localhost:44325/";
        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Registration(users user, string confirmPassword)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    // Additional validations
                    if (!string.IsNullOrWhiteSpace(user.name) && user.name.Length <= 50)
                    {
                        if (Regex.IsMatch(user.name, @"^[a-zA-ZñÑ]+$"))
                        {
                            if (!string.IsNullOrWhiteSpace(user.first_surname) && user.first_surname.Length <= 50)
                            {
                                if (Regex.IsMatch(user.first_surname, @"^[a-zA-ZñÑ]+$"))
                                {
                                    if (!string.IsNullOrWhiteSpace(user.second_surname) && user.second_surname.Length <= 50)
                                    {
                                        if (Regex.IsMatch(user.second_surname, @"^[a-zA-ZñÑ]+$"))
                                        {
                                            if (user.phone > 0 && user.phone.ToString().Length <= 10 && Regex.IsMatch(user.phone.ToString(), @"^[0-9]+$"))
                                            {
                                                if (!string.IsNullOrWhiteSpace(user.email) && user.email.Length <= 100)
                                                {
                                                    string emailPattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
                                                    if (Regex.IsMatch(user.email, emailPattern))
                                                    {
                                                        if (db.users.Any(u => u.email == user.email))
                                                        {
                                                            ModelState.AddModelError("email", "Este email ya está registrado.");
                                                        }
                                                        else
                                                        {
                                                            if (!string.IsNullOrWhiteSpace(user.password) && user.password.Length >= 8)
                                                            {
                                                                if (user.password == confirmPassword)
                                                                {
                                                                    string activationToken = GenerateActivationToken(user);
                                                                    string hashedPassword = HashPassword(user.password.Trim().Replace(" ", ""));
                                                                    user.email = user.email.Trim().Replace(" ", "");
                                                                    user.isActive = false;
                                                                    user.password = hashedPassword;
                                                                    user.role_id = 3;
                                                                    user.activation_token = activationToken;
                                                                    db.users.Add(user);
                                                                    db.SaveChanges();

                                                                    TempData["ActivationEmail"] = user.email;
                                                                    SendActivationMail(user.email, activationToken);

                                                                    return RedirectToAction("ActivationSent");
                                                                }
                                                                else
                                                                {
                                                                    ModelState.AddModelError("confirmPassword", "Las contraseñas no coinciden.");
                                                                }
                                                            }
                                                            else
                                                            {
                                                                ModelState.AddModelError("password", "La contraseña es requerida y debe tener al menos 8 caracteres.");
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        ModelState.AddModelError("email", "El correo electrónico no tiene un formato válido.");
                                                    }
                                                }
                                                else
                                                {
                                                    ModelState.AddModelError("email", "El correo es requerido y debe tener como máximo 100 caracteres.");
                                                }
                                            }
                                            else
                                            {
                                                ModelState.AddModelError("phone", "El teléfono es requerido y debe tener como máximo 10 caracteres.");
                                            }
                                        }
                                        else
                                        {
                                            ModelState.AddModelError("second_surname", "El segundo apellido debe contener únicamente letras sin espacios.");
                                        }
                                    }
                                    else
                                    {
                                        ModelState.AddModelError("second_surname", "El segundo apellido es requerido y debe tener como máximo 50 caracteres.");
                                    }
                                }
                                else
                                {
                                    ModelState.AddModelError("first_surname", "El primer apellido debe contener únicamente letras sin espacios.");
                                }
                            }
                            else
                            {
                                ModelState.AddModelError("first_surname", "El primer apellido es requerido y debe tener como máximo 50 caracteres.");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("name", "El nombre debe contener únicamente letras sin espacios.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("name", "El nombre es requerido y debe tener como máximo 50 caracteres.");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Ha ocurrido un error durante el registro. Por favor, inténtalo de nuevo más tarde.");
                // You can log the exception details here for debugging purposes
            }
            return View(user);
        }

        public ActionResult ActivationSent()
        {
            string activationEmail = TempData["ActivationEmail"] as string;
            ViewBag.ActivationEmail = activationEmail;
            return View();
        }

        [HttpGet]
        public ActionResult ActivateAccount(string token)
        {
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ActivateAccount(ActivateAccountViewModel model, string token)
        {

            ActivateAccountViewModel tokenModel = new ActivateAccountViewModel();
            tokenModel.Token = token;

            if (ModelState.IsValid)
            {
                var user = db.users.FirstOrDefault(u => u.activation_token == token);

                if (string.IsNullOrEmpty(user.email))
                {
                    ModelState.AddModelError("email", "El correo electrónico no puede quedar vacío.");
                }
                else
                {
                    string emailPattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
                    if (Regex.IsMatch(user.email, emailPattern))
                    {
                        ModelState.AddModelError("email", "El correo electrónico no tiene un formato válido.");
                    }
                }

                if (string.IsNullOrEmpty(user.password))
                {
                    ModelState.AddModelError("password", "La contraseña no puede quedar vacío.");
                }
                else
                {
                    string passwordPattern = @"^(?=.*[!@#$%^&*(),.?"":{}|<>])(?=.*[A-Z])(?=.*\d).+$";
                    if (!Regex.IsMatch(user.password, passwordPattern))
                    {
                        ModelState.AddModelError("password", "La contraseña debe contener al menos un carácter especial, una mayúscula y un número.");
                    }
                }

                if (user == null)
                {
                    Alert("Esta cuenta ya se encuentra activa o el token ha expirado", NotificationTypeMessage.Advertencia, NotificationType.warning);

                    ModelState.AddModelError("", "Esta cuenta ya se encuentra activa o el token ha expirado");
                    return View(model);
                }

                var comparePass = VerifyPassword(model.Password, user.password.Trim().Replace(" ", ""));

                if ((user.email.Trim() == model.Email) && comparePass)
                {
                    user.isActive = true;
                    user.activation_token = null;
                    db.SaveChanges();

                    FormsAuthentication.SetAuthCookie(model.Email, false);

                    return RedirectToAction("Index", "User");
                }
                else
                {
                    Alert("Correo o contraseña incorrectos", NotificationTypeMessage.Advertencia, NotificationType.warning);

                    ModelState.AddModelError("", "Correo o contraseña incorrectos.");
                }
            }

            ViewBag.Token = token;
            return View(model);
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(users user)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var loginUser = db.users.SingleOrDefault(u => u.email.Trim() == user.email);

                    if (string.IsNullOrEmpty(user.email))
                    {
                        ModelState.AddModelError("email", "El campo 'Correo' es requerido.");
                    }
                    else
                    {
                        string emailPattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
                        if (!Regex.IsMatch(user.email, emailPattern))
                        {
                            ModelState.AddModelError("email", "El correo electrónico no tiene un formato válido.");
                        }
                    }
                    if (string.IsNullOrEmpty(user.password))
                    {
                        ModelState.AddModelError("password", "La contraseña es requerida.");
                        return View(user);
                    }

                    if (loginUser != null)
                    {
                        bool isActive = loginUser.isActive;

                        if (!isActive)
                        {
                            Alert("Esta cuenta no se encuentra activa. Regístrese o revise su correo para activarla", NotificationTypeMessage.Error, NotificationType.error);
                            ModelState.AddModelError("", "Esta cuenta no se encuentra activa. Regístrese o revise su correo para activarla.");
                            return View(user);
                        }

                        // Verificar la contraseña utilizando una función hash segura
                        bool isPasswordValid = VerifyPassword(user.password, loginUser.password);

                        if (isPasswordValid)
                        {
                            Session["LoggedInUser"] = loginUser;
                            Session["Email"] = loginUser.email;
                            if (loginUser.role_id == 1)
                            {
                                Session["Rol"] = "administrator";
                            }
                            else if (loginUser.role_id == 2)
                            {
                                Session["Rol"] = "user";
                            }
                            else if (loginUser.role_id == 3)
                            {
                                Session["Rol"] = "customer";
                            }
                            Alert("Acceso Exitoso", NotificationTypeMessage.Éxito, NotificationType.success);
                            // Store the username in a session variable
                            Session["Username"] = loginUser.name;
                            return RedirectToAction("Index", "User");
                        }
                    }
                    else
                    {
                        Alert("El usuario no existe.", NotificationTypeMessage.Error, NotificationType.error);
                        ModelState.AddModelError("", "El usuario no existe.");
                        return View(user);
                    }
                    Alert("Credenciales incorrectas", NotificationTypeMessage.Error, NotificationType.error);
                    ModelState.AddModelError("", "Credenciales incorrectas.");
                    return View(user);
                }

                return View(user);
            }
            catch (Exception ex)
            {
                Alert("Ha ocurrido un error durante el proceso de inicio de sesión", NotificationTypeMessage.Error, NotificationType.error);
                ModelState.AddModelError("", "Ha ocurrido un error durante el proceso de inicio de sesión " + ex.Message);
                return View(user);
            }
        }

        public ActionResult Logout()
        {

            Session.Clear();
            Alert("Sesión Terminada.", NotificationTypeMessage.Éxito, NotificationType.success);
            return RedirectToAction("Index", "User");
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpGet]
        public ActionResult StartRecovery(string token)
        {
            Models.ViewModel.RecoveryPasswordViewModel model = new Models.ViewModel.RecoveryPasswordViewModel();
            model.token = token;
            using (carwashEntities db = new carwashEntities())
            {
                if (model.token == null || model.token.Trim().Equals(""))
                {
                    ViewBag.Error = "El token ha expirado";
                    return View("StartRecovery");
                }
                var oUser = db.users.Where(d => d.token_recovery == model.token).FirstOrDefault();
                if (oUser == null)
                {
                    ViewBag.Error = "El token ha expirado";
                    return View("StartRecovery");
                }
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Recovery(Models.ViewModel.RecoveryPasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("StartRecovery", model);
                }

                using (Models.carwashEntities db = new Models.carwashEntities())
                {
                    var oUser = db.users.Where(d => d.token_recovery == model.token).FirstOrDefault();
                    string passwordPattern = @"^(?=.*[!@#$%^&*(),.?"":{}|<>])(?=.*[A-Z])(?=.*\d).+$";
                    if (!Regex.IsMatch(model.Password, passwordPattern))
                    {
                        ModelState.AddModelError("", "La contraseña debe contener al menos un carácter especial, una mayúscula y un número.");
                        return View("StartRecovery", model);
                    }
                    if (oUser != null)
                    {
                        oUser.password = HashPassword(model.Password.Trim().Replace(" ", ""));
                        oUser.token_recovery = null;
                        db.SaveChanges();
                        Alert("Contraseña modificada con exito", NotificationTypeMessage.Éxito, NotificationType.success);
                        ViewBag.Message = "Contraseña modificada con exito";
                    }
                }
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                Alert("Ha ocurrido un error durante el proceso de recuperacion de contraseña", NotificationTypeMessage.Error, NotificationType.error);
                ModelState.AddModelError("", "Ha ocurrido un error durante el proceso de inicio de sesión " + ex.Message);
                return View("StartRecovery", model);
            }
        }

        [HttpGet]
        public ActionResult PasswordRecovery()
        {
            Models.ViewModel.RecoveryViewModel model = new Models.ViewModel.RecoveryViewModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult PasswordRecovery(Models.ViewModel.RecoveryViewModel model, users user)
        {
            try
            {
                if (!ModelState.IsValid)
                {

                    if (string.IsNullOrEmpty(user.email))
                    {
                        ModelState.AddModelError("email", "El correo electrónico no puede quedar vacío.");
                    }
                    else
                    {
                        string emailPattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
                        if (!Regex.IsMatch(user.email, emailPattern))
                        {
                            ModelState.AddModelError("email", "El correo electrónico no tiene un formato válido.");
                        }
                    }

                    return View("ForgotPassword", model);
                }

                string token = GetSha256(Guid.NewGuid().ToString());

                using (carwashEntities db = new carwashEntities())
                {
                    var oUser = db.users.Where(d => d.email == model.Email).FirstOrDefault();
                    if (oUser != null)
                    {
                        oUser.token_recovery = token;
                        db.Entry(oUser).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        // Enviar mail
                        SendEmail(oUser.email, token);
                    }
                    else
                    {
                        // El correo de recuperación no existe en la base de datos
                        ModelState.AddModelError("Email", "El correo proporcionado no existe en nuestra base de datos.");
                        return View("ForgotPassword", model);
                    }

                }

                return View();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }


        public ActionResult UsersList()
        {
            try
            {
                string rol = Session["Rol"] as string;

                if (rol.Equals("administrator"))
                {
                    List<users> users = db.users.ToList(); // Obtener todos los usuarios de la tabla

                    return View(users);
                }
                else
                {
                    return RedirectToAction("Login", "User");
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);

            }

        }

        public ActionResult UpdateAll(string[] lista)
        {
            string rol = Session["Rol"] as string;

            if (rol.Equals("administrator"))
            {
                var users = db.users.ToList(); // Obtener todos los usuarios de la base de datos

                foreach (var user in users)
                {
                    int newRoleId = int.Parse(lista[users.IndexOf(user)]);

                    user.role_id = newRoleId;

                    db.users.Attach(user);
                    db.Entry(user).State = EntityState.Modified;
                }

                db.SaveChanges();

                Alert("Rol de Usuario Actualizado", NotificationTypeMessage.Éxito, NotificationType.success);
                return RedirectToAction("UsersList");
            }
            else
            {
                return RedirectToAction("Login", "User");
            }
        }

        public ActionResult Edit(int id)
        {
            var users = db.users.SingleOrDefault(u => u.user_id == id);
            return View(users);
        }

        public ActionResult Delete(int id)
        {
            string rol = Session["Rol"] as string;

            if (rol.Equals("administrator"))
            {
                var lg = db.users.Where(a => a.user_id.Equals(id)).FirstOrDefault();

                db.users.Remove(lg);
                db.SaveChanges();
                Alert("Usuario Borrado", NotificationTypeMessage.Éxito, NotificationType.success);
                return RedirectToAction("UsersList");
            }
            else
            {
                return RedirectToAction("Login", "User");
            }


        }

        [HttpPost]
        public JsonResult NotificationsList()
        {
            string rol = Session["Rol"] as string;

            if (rol == "administrator")
            {
                List<notifications> notifications = db.notifications.ToList();
                return Json(notifications);
            }

            return Json(new { errorMessage = "No tiene permisos de administrador." });
        }

        [HttpPost]
        public JsonResult DeleteNotification(int notification_id)
        {
            try
            {
                var notification = db.notifications.Find(notification_id);
                if (notification != null)
                {
                    db.notifications.Remove(notification);
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Notificación no encontrada." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar la notificación." });
            }
        }

        public ActionResult UserEdit()
        {
            return View();
        }

        public ActionResult UserEdition()
        {
            var loggedInUser = Session["LoggedInUser"] as users;
            return View(loggedInUser);
        }

        [HttpPost]
        public ActionResult UpdateProfile(users updatedUser)
        {
            var loggedInUser = Session["LoggedInUser"] as users;
            //if (string.IsNullOrEmpty(updatedUser.name))
            //{
            //    ModelState.AddModelError("name", "El nombre es requerido.");
            //}
            //else if (!Regex.IsMatch(updatedUser.name, @"^[a-zA-ZñÑ]+$"))
            //{
            //    ModelState.AddModelError("name", "El nombre debe contener únicamente letras.");
            //}
            if (updatedUser.phone == 0)
            {
                ModelState.AddModelError("phone", "El telefono es requerido.");
            }
                if (ModelState.IsValid)
            {
                using (var db = new carwashEntities())
                {
                    var userToUpdate = db.users.Find(loggedInUser.user_id);

                    if (userToUpdate != null)
                    {
                        userToUpdate.phone = updatedUser.phone;
                        userToUpdate.name = updatedUser.name;

                        db.Entry(userToUpdate).State = EntityState.Modified;
                        db.SaveChanges();
                        Alert("Datos actualizados", NotificationTypeMessage.Éxito, NotificationType.success);
                        Session["LoggedInUser"] = userToUpdate;
                        return RedirectToAction("UserEdit", "User");
                    }
                }
            }

            return View("UserEdition", updatedUser);
        }

        #region HELPERS
        private string GenerateActivationToken(users user)
        {
            string token = Guid.NewGuid().ToString();
            return token;
        }

        private bool VerifyPassword(string enteredPassword, string storedHash)
        {

            byte[] hashBytes = Convert.FromBase64String(storedHash);

            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, 10000);
            byte[] enteredHash = pbkdf2.GetBytes(20);

            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != enteredHash[i])
                    return false;
            }

            return true;
        }

        private string HashPassword(string password)
        {

            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);


            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);


            byte[] hash = pbkdf2.GetBytes(20);


            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);


            string hashedPassword = Convert.ToBase64String(hashBytes);

            return hashedPassword;
        }
        private string GetSha256(string str)
        {
            SHA256 sha256 = SHA256Managed.Create();
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] stream = null;
            StringBuilder sb = new StringBuilder();
            stream = sha256.ComputeHash(encoding.GetBytes(str));
            for (int i = 0; i < stream.Length; i++) sb.AppendFormat("{0:x2}", stream[i]);
            return sb.ToString();
        }

        private void SendEmail(string toAddress, string token)
        {
            string fromAddress = "vettelcarwash@gmail.com";
            string Contrasena = "eyvmzrwsuhxwruec";
            string url = "http://www.vettelcarwash.somee.com/User/StartRecovery/?token=" + token;
            string subject = "Recuperación de contraseña";
            string body = "<p>Correo para recuperación de contraseña</p><br>" +
                "<a href='" + url + "'>Click para recuperar</a>";


            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(fromAddress, Contrasena);

            MailMessage message = new MailMessage(fromAddress, toAddress, subject, body);
            message.IsBodyHtml = true;

            client.Send(message);

            client.Dispose();
        }

        private void SendActivationMail(string toAddress, string activationToken)
        {
            string fromAddress = "vettelcarwash@gmail.com";
            string Contrasena = "eyvmzrwsuhxwruec";
            string url = "http://www.vettelcarwash.somee.com//User/ActivateAccount/?token=" + activationToken;
            string subject = "Activación de cuenta - Carwash";
            string body = "<p>Hola! Gracias por registrarte en nuestro sitio web, para activar tu cuenta por favor ingresa a este link:</p><br>" +
                "<a href='" + url + "'>Click aquí para activar su cuenta</a>";


            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(fromAddress, Contrasena);

            MailMessage message = new MailMessage(fromAddress, toAddress, subject, body);
            message.IsBodyHtml = true;

            client.Send(message);

            client.Dispose();
        }
        #endregion

    }
}
