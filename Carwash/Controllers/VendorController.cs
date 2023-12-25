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
    public class VendorController : Controller
    {
        carwashEntities db = new carwashEntities();
        string urlDomain = "https://localhost:44325/";
        public ActionResult Index()
        {
            List<Vendors> vendors = db.Vendors.ToList(); // Obtener todos los productos de la tabla
            return View(vendors);
        }


        public ActionResult Edit(int id)
        {
            Vendors vendor = db.Vendors.SingleOrDefault(u => u.Id == id);
            return View(vendor);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(Vendors feedback)
        {
            if (ModelState.IsValid)
            {


                db.Vendors.Attach(feedback);
                db.Entry(feedback).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");

            }

            return View(feedback);
        }

        public ActionResult Create()
        {

            return View();
        }

        [HttpPost]
        public ActionResult Create(Vendors vendor)
        {
            if (ModelState.IsValid || true)
            {
                //string imageBase64 = Convert.ToBase64String(feedback.ImagenFile);

                /*using (var ms = new MemoryStream())
                {
                    feedback.ImagenFile.CopyTo(ms);
                    feedback.product_image = ms.ToArray();
                }*/

                db.Vendors.Add(vendor);
                db.SaveChanges();
                return RedirectToAction("Index");

            }

            return View(vendor);

        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                // List<product_stock> products= db.product_stock.ToList();

                using (carwashEntities dbContext = new carwashEntities())
                {
                    List<product_stock> products = dbContext.product_stock.ToList();
                    foreach (product_stock product in products)
                    {
                        if (product.id_vendor == id)
                        {
                            dbContext.product_stock.Remove(product);
                            dbContext.SaveChanges();
                        }
                    }
                }
                Vendors material = db.Vendors.Find(id);

                if (material != null)
                {

                    db.Vendors.Remove(material);
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