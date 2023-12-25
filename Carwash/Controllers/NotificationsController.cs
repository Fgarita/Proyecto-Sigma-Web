using Carwash.Models;
using Carwash.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Carwash.Controllers
{
    public class NotificationsController : Controller
    {
        carwashEntities db = new carwashEntities();
        // GET: Notifications
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult NotificationsList()
        {
            string rol = Session["Rol"] as string;

            if (rol.Equals("administrator"))
            {
                List<Carwash.Models.notifications> notifications = db.notifications.ToList();
                var viewModel = new NotificationsViewModel
                {
                    Notifications = notifications
                };

                return View(viewModel);
            }

            return View();
        }

    }
}
