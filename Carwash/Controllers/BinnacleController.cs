using Carwash.Models;
using Carwash.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Carwash.Controllers
{
    public class BinnacleController : Controller
    {
        carwashEntities db = new carwashEntities();
        string urlDomain = "https://localhost:44325/";
        public ActionResult Index(int id)
        {

            List<Binnacle> binnacle = new List<Binnacle>();
            if (id == 1)
            {
                //binnacle = db.Binnacle.ToList();
                binnacle = db.Binnacle.Where(item => item.type == "Gasto").ToList();
            }
            else {
                binnacle = db.Binnacle.Where(item => item.type == "Ingreso").ToList();
            }
            return View(binnacle);
        }

     
        public ActionResult Edit(int id)
        {
            Binnacle binnacle=  db.Binnacle.SingleOrDefault(u => u.idBinnacle== id);
            return View(binnacle);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(Binnacle binnacle)
        {
            if (ModelState.IsValid)
            {
                // Perform additional validations
                if (binnacle.type == "Ingreso" || binnacle.type == "Gasto")
                {
                    if (!string.IsNullOrWhiteSpace(binnacle.description) && binnacle.description.Length <= 100)
                    {
                        if (binnacle.amount >= 0)
                        {

                            db.Binnacle.Attach(binnacle);
                            db.Entry(binnacle).State = EntityState.Modified;
                            db.SaveChanges();
                            if (binnacle.type.Equals("Ingreso"))
                                return RedirectToAction("Index", new { id = 2 });
                            else
                                return RedirectToAction("Index", new { id = 1 });
                        }
                        else
                        {
                            ModelState.AddModelError("amount", "El monto debe ser mayor o igual a cero.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("description", "La descripción es requerida y debe tener como máximo 100 caracteres.");
                    }
                }
                else
                {
                    ModelState.AddModelError("type", "Seleccione un tipo válido (Ingreso o Gasto).");
                }
            }

            return View("Edit", binnacle); // Return to the Create view with validation errors
        }

        [HttpPost]
        public ActionResult Create(Binnacle binnacle)
        {
            if (ModelState.IsValid || true)
            {
                // Perform additional validations
                if (binnacle.type == "Ingreso" || binnacle.type == "Gasto")
                {
                    if (!string.IsNullOrWhiteSpace(binnacle.description) && binnacle.description.Length <= 100)
                    {
                        if (binnacle.amount >= 0)
                        {

                            db.Binnacle.Add(binnacle);
                            db.SaveChanges();
                            if (binnacle.type.Equals("Ingreso"))
                                return RedirectToAction("Index", new { id = 2 });
                            else
                                return RedirectToAction("Index", new { id = 1 });
                        }
                        else
                        {
                            ModelState.AddModelError("amount", "El monto debe ser mayor o igual a cero.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("description", "La descripción es requerida y debe tener como máximo 100 caracteres.");
                    }
                }
                else
                {
                    ModelState.AddModelError("type", "Seleccione un tipo válido (Ingreso o Gasto).");
                }
            }

            return View("Create", binnacle); // Return to the Create view with validation errors
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
               
                Binnacle binnacle= db.Binnacle.Find(id);
                string tipo = binnacle.type;

                if (binnacle != null)
                {

                    db.Binnacle.Remove(binnacle);
                    db.SaveChanges();
                }
                if (tipo.Equals("Ingreso"))
                    return RedirectToAction("Index", new { id = 2 });
                else
                    return RedirectToAction("Index", new { id = 1 });
                
            }
            catch
            {
                return View();
            }
        }
    }
}