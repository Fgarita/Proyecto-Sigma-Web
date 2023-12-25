using AppWebWithSweetAlert.Controllers;
using Carwash.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static Carwash.Models.Enum;

namespace Carwash.Controllers
{
    public class ProductController : BaseController
    {
        carwashEntities db = new carwashEntities();
        //string urlDomain = "https://localhost:44325/";
        public ActionResult Index()
        {
            List<product_stock> products = db.product_stock.ToList();
            return View(products);
        }

        public ActionResult GetImage(int productId)
        {
            var product = db.product_stock.Find(productId);
            if (product != null && product.product_image != null)
            {
                return File(product.product_image, "image/png");
            }

            return HttpNotFound();
        }

        public ActionResult Edit(int id)
        {
            product_stock producto = db.product_stock.SingleOrDefault(u => u.id_stock == id);
            return View(producto);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(product_stock model, HttpPostedFileBase imageFile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (!string.IsNullOrWhiteSpace(model.product_name))
                    {
                        if (imageFile != null && imageFile.ContentLength != 0)
                        {
                            if (model.price > 0)
                            {
                                if (model.id_vendor != 0)
                                {
                                    if (model.quantity > 0)
                                    {
                                        if (!string.IsNullOrWhiteSpace(model.description))
                                        {
                                            if (imageFile != null && imageFile.ContentLength > 0)
                                            {
                                                byte[] imageData = new byte[imageFile.ContentLength];
                                                using (BinaryReader binaryReader = new BinaryReader(imageFile.InputStream))
                                                {
                                                    imageData = binaryReader.ReadBytes(imageFile.ContentLength);
                                                }

                                                model.product_image = imageData;
                                            }
                                            Vendors vendor = db.Vendors.SingleOrDefault(u => u.Id == model.id_vendor);
                                            model.vendor_name = vendor.Name;
                                            db.product_stock.Attach(model);
                                            db.Entry(model).State = EntityState.Modified;
                                            db.SaveChanges();
                                            Alert("Producto Actualizo", NotificationTypeMessage.Éxito, NotificationType.success);
                                            return RedirectToAction("Index");
                                        }
                                        else
                                        {
                                            ModelState.AddModelError("description", "La descripción es requerida.");
                                        }
                                    }
                                    else
                                    {
                                        ModelState.AddModelError("quantity", "La cantidad debe ser mayor que cero.");
                                    }
                                }
                                else
                                {
                                    ModelState.AddModelError("id_vendor", "Debe seleccionar un proveedor.");
                                }
                            }
                            else
                            {
                                ModelState.AddModelError("price", "El precio debe ser mayor que cero.");

                            }
                        }
                        else
                        {
                            ModelState.AddModelError("product_image", "Debe cargar una imagen.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("product_name", "El nombre es requerido.");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Ha ocurrido un error durante el registro. Por favor, inténtalo de nuevo más tarde.");
            }
            return View(model);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(product_stock model, HttpPostedFileBase imageFile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (!string.IsNullOrWhiteSpace(model.product_name))
                    {
                        if (imageFile != null && imageFile.ContentLength != 0)
                        {
                            if (model.price > 0)
                            {
                                if (model.id_vendor != 0)
                                {
                                    if (model.quantity > 0)
                                    {
                                        if (!string.IsNullOrWhiteSpace(model.description))
                                        {
                                            if (imageFile == null && imageFile.ContentLength == 0)
                                            {
                                                byte[] imageData = new byte[imageFile.ContentLength];
                                                using (BinaryReader binaryReader = new BinaryReader(imageFile.InputStream))
                                                {
                                                    imageData = binaryReader.ReadBytes(imageFile.ContentLength);
                                                }

                                                model.product_image = imageData;
                                            }
                                            Vendors vendor = db.Vendors.SingleOrDefault(u => u.Id == model.id_vendor);
                                            model.vendor_name = vendor.Name;
                                            db.product_stock.Add(model);
                                            db.SaveChanges();
                                            Alert("Producto Agregado al Inventario", NotificationTypeMessage.Éxito, NotificationType.success);
                                            return RedirectToAction("Index");
                                        }
                                        else
                                        {
                                            ModelState.AddModelError("description", "La descripción es requerida.");
                                        }
                                    }
                                    else
                                    {
                                        ModelState.AddModelError("quantity", "La cantidad debe ser mayor que cero.");
                                    }
                                }
                                else
                                {
                                    ModelState.AddModelError("id_vendor", "Debe seleccionar un proveedor.");
                                }
                            }
                            else
                            {
                                ModelState.AddModelError("price", "El precio debe ser mayor que cero.");

                            }
                        }
                        else
                        {
                            ModelState.AddModelError("product_image", "Debe cargar una imagen.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("product_name", "El nombre es requerido.");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Ha ocurrido un error durante el registro. Por favor, inténtalo de nuevo más tarde.");
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                product_stock material = db.product_stock.Find(id);

                if (material != null)
                {

                    db.product_stock.Remove(material);
                    db.SaveChanges();
                    Alert("Producto Borrado", NotificationTypeMessage.Éxito, NotificationType.success);
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