using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using AppWebWithSweetAlert.Controllers;
using Carwash.Models;
using static Carwash.Models.Enum;

namespace Carwash.Controllers
{
    public class ServiceController : BaseController
    {
        carwashEntities db = new carwashEntities();
        // GET: Service
        public ActionResult Index()
        {
            List<service> vendors = db.service.ToList(); // Obtener todos los servicios de la tabla
            return View(vendors);
        }

        // GET: Service/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Service/Create
        [HttpPost]
        public ActionResult Create(service Services)
        {
            try
            {
                if (ModelState.IsValid || true)
                {

                    db.service.Add(Services);
                    db.SaveChanges();
                    Alert("Servicio Creado", NotificationTypeMessage.Éxito, NotificationType.success);
                    return RedirectToAction("Index");
                }
            }
            catch
            {
                return View();
            }
            return View(Services);
        }

        // GET: Service/Edit/5
        public ActionResult Edit(int id)
        {
            service Services = db.service.SingleOrDefault(u => u.service_id == id);
            return View(Services);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(service Services)
        {
            try
            {
                if (ModelState.IsValid)
            {
                db.service.Attach(Services);
                db.Entry(Services).State = EntityState.Modified;
                db.SaveChanges();
                Alert("Servicio actualizado", NotificationTypeMessage.Éxito, NotificationType.success);
                return RedirectToAction("Index");

            }
            }
            catch
            {
                return View();
             }
            return View(Services);
        }

        public ActionResult Delete(int id)
        {
            try
            {
                // List<product_stock> products= db.product_stock.ToList();

                using (carwashEntities dbContext = new carwashEntities())
                {
                    List<appointments> Appointments = dbContext.appointments.ToList();
                    foreach (appointments Appointment in Appointments)
                    {
                        if (Appointment.service_id == id)
                        {
                            dbContext.appointments.Remove(Appointment);
                            dbContext.SaveChanges();
                        }
                    }
                }
                service material = db.service.Find(id);

                if (material != null)
                {

                    db.service.Remove(material);
                    db.SaveChanges();
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}