using ArchivosPlanosWebV2._5.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ArchivosPlanosWebV2._5.Controllers
{
    public class CRUDController : Controller
    {
        private AppDbContextSQL db = new AppDbContextSQL();
        public ActionResult Index()
        {
            var Data_Tramo = db.Type_Delegacion.ToList();
            ModeloVirtual modelovirtual = new ModeloVirtual();
            modelovirtual.Delegaciones = new List<SQL_Delegacion>();
            modelovirtual.Plazas = new List<SQL_Plaza>();
            modelovirtual.Carriles = new List<SQL_Carril>();
            modelovirtual.Operadores = new List<SQL_Operadores>();
            foreach (var item in Data_Tramo)
            {
                modelovirtual.Delegaciones.Add(new SQL_Delegacion
                {
                    Id_Delegacion = item.Id_Delegacion,
                    Num_Delegacion = item.Num_Delegacion,
                    Nom_Delegacion = item.Nom_Delegacion
                }
                );
            }
            return View(modelovirtual);

        }
        public ActionResult DetailsDelegacion(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Type_Delegacion type_Delegacion = db.Type_Delegacion.Find(id);
            if (type_Delegacion == null)
            {
                return HttpNotFound();
            }
            return View(type_Delegacion);
        }
        public ActionResult CreateDelegacion()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateDelegacion([Bind(Include = "Id_Delegacion,Num_Delegacion,Nom_Delegacion")] Type_Delegacion type_Delegacion)
        {
            if (ModelState.IsValid)
            {
                db.Type_Delegacion.Add(type_Delegacion);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(type_Delegacion);
        }
        public ActionResult EditDelegacion(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Type_Delegacion type_Delegacion = db.Type_Delegacion.Find(id);
            if (type_Delegacion == null)
            {
                return HttpNotFound();
            }
            return View(type_Delegacion);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditDelegacion([Bind(Include = "Id_Delegacion,Num_Delegacion,Nom_Delegacion")] Type_Delegacion type_Delegacion)
        {
            if (ModelState.IsValid)
            {
                db.Entry(type_Delegacion).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(type_Delegacion);
        }
        public ActionResult DeleteDelegacion(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Type_Delegacion type_Delegacion = db.Type_Delegacion.Find(id);
            if (type_Delegacion == null)
            {
                return HttpNotFound();
            }
            return View(type_Delegacion);
        }
        [HttpPost, ActionName("DeleteDelegacion")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteDelegacionConfirmed(int id)
        {
            Type_Delegacion type_Delegacion = db.Type_Delegacion.Find(id);
            var DataCarriles = db.Type_Carril.ToList();
            var DataOperadores = db.Type_Operadores.ToList();
            var DataPlazas = db.Type_Plaza.ToList();
            int id_pla = 0;
            foreach (var item in DataPlazas)
            {
                if (item.Delegacion_Id == id)
                {
                    id_pla = item.Id_Plaza;
                    break;
                }
            }
            foreach (var item in DataCarriles)
            {
                if (item.Plaza_Id == id_pla)
                {
                    db.Type_Carril.Remove(item);
                    db.SaveChanges();
                }
            }
            foreach (var item in DataOperadores)
            {
                if (item.Plaza_Id == id_pla)
                {
                    db.Type_Operadores.Remove(item);
                    db.SaveChanges();
                }
            }
            foreach (var item in DataPlazas)
            {
                if (item.Delegacion_Id == id)
                {
                    db.Type_Plaza.Remove(item);
                    db.SaveChanges();
                }
            }
            db.Type_Delegacion.Remove(type_Delegacion);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public ActionResult VerPlaza(int Id_Del)
        {
            var Data_Tramo = db.Type_Delegacion.ToList();
            var plazasdetramo = db.Type_Delegacion.Join(db.Type_Plaza, del => del.Id_Delegacion, pla => pla.Delegacion_Id, (del, pla) => new { del, pla }).Where(x => x.pla.Delegacion_Id == Id_Del).ToList();
            ModeloVirtual modelovirtual = new ModeloVirtual();
            modelovirtual.Delegaciones = new List<SQL_Delegacion>();
            modelovirtual.Plazas = new List<SQL_Plaza>();
            modelovirtual.Carriles = new List<SQL_Carril>();
            modelovirtual.Operadores = new List<SQL_Operadores>();
            foreach (var item in Data_Tramo)
            {
                if (item.Id_Delegacion == Id_Del)
                {
                    modelovirtual.Delegaciones.Add(new SQL_Delegacion
                    {
                        Id_Delegacion = item.Id_Delegacion,


                        Nom_Delegacion = item.Nom_Delegacion,
                        Num_Delegacion = item.Num_Delegacion
                    });
                }
            }
            foreach (var item in plazasdetramo)
            {
                modelovirtual.Plazas.Add(new SQL_Plaza
                {
                    Id_Plaza = item.pla.Id_Plaza,
                    Nom_Plaza = item.pla.Nom_Plaza,
                    Num_Plaza = item.pla.Num_Plaza,
                    Delegacion_Id = item.pla.Delegacion_Id
                });
            }
            if (plazasdetramo.Count == 0)
                ViewBag.SinPlazas = "No hay plazas en la " + modelovirtual.Delegaciones.FirstOrDefault().Nom_Delegacion;
            return View("Index", modelovirtual);
        }
        public ActionResult DetailsPlaza(int? id, int Id_Del)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Type_Plaza type_Plaza = db.Type_Plaza.Find(id);
            ViewBag.SelectedDelegacion = Id_Del;
            if (type_Plaza == null)
            {
                return HttpNotFound();
            }
            return View(type_Plaza);
        }
        public ActionResult CreatePlaza(int Id_Del)
        {
            ViewBag.Delegacion_Id = new SelectList(db.Type_Delegacion, "Id_Delegacion", "Nom_Delegacion");
            ViewBag.SelectedDelegacion = Id_Del;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePlaza([Bind(Include = "Id_Plaza,Num_Plaza,Nom_Plaza,Delegacion_Id")] Type_Plaza type_Plaza)
        {
            if (ModelState.IsValid)
            {
                db.Type_Plaza.Add(type_Plaza);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Delegacion_Id = new SelectList(db.Type_Delegacion, "Id_Delegacion", "Nom_Delegacion", type_Plaza.Delegacion_Id);
            return View(type_Plaza);
        }
        public ActionResult EditPlaza(int? id, int Id_Del)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Type_Plaza type_Plaza = db.Type_Plaza.Find(id);
            ViewBag.SelectedDelegacion = Id_Del;
            if (type_Plaza == null)
            {
                return HttpNotFound();
            }
            ViewBag.Delegacion_Id = new SelectList(db.Type_Delegacion, "Id_Delegacion", "Nom_Delegacion", type_Plaza.Delegacion_Id);
            return View(type_Plaza);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPlaza([Bind(Include = "Id_Plaza,Num_Plaza,Nom_Plaza,Delegacion_Id")] Type_Plaza type_Plaza)
        {
            if (ModelState.IsValid)
            {
                db.Entry(type_Plaza).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Delegacion_Id = new SelectList(db.Type_Delegacion, "Id_Delegacion", "Nom_Delegacion", type_Plaza.Delegacion_Id);
            return View(type_Plaza);
        }
        public ActionResult DeletePlaza(int? id, int Id_Del)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Type_Plaza type_Plaza = db.Type_Plaza.Find(id);
            ViewBag.SelectedDelegacion = Id_Del;
            if (type_Plaza == null)
            {
                return HttpNotFound();
            }
            return View(type_Plaza);
        }
        [HttpPost, ActionName("DeletePlaza")]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePlazaConfirmed(int id)
        {
            Type_Plaza type_Plaza = db.Type_Plaza.Find(id);
            var DataCarriles = db.Type_Carril.ToList();
            var DataOperadores = db.Type_Operadores.ToList();
            foreach (var item in DataCarriles)
            {
                if (item.Plaza_Id == id)
                {
                    db.Type_Carril.Remove(item);
                }
            }
            foreach (var item in DataOperadores)
            {
                if (item.Plaza_Id == id)
                {
                    db.Type_Operadores.Remove(item);
                }
            }
            db.Type_Plaza.Remove(type_Plaza);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult VerCarril(int IdPlaza)
        {
            var Data_Delegacion = db.Type_Delegacion.ToList();
            var DataPlaza = db.Type_Plaza.ToList();
            var Data_Carril = db.Type_Carril.ToList();
            ModeloVirtual modelovirtual = new ModeloVirtual();
            modelovirtual.Delegaciones = new List<SQL_Delegacion>();
            modelovirtual.Plazas = new List<SQL_Plaza>();
            modelovirtual.Carriles = new List<SQL_Carril>();
            modelovirtual.Operadores = new List<SQL_Operadores>();
            var CarrilesDePlaza = db.Type_Plaza.Join(db.Type_Carril, pla => pla.Id_Plaza, car => car.Plaza_Id, (pla, car) => new { pla, car }).Where(x => x.car.Plaza_Id == IdPlaza).ToList();
            if (CarrilesDePlaza.Count != 0)
            {
                var Id_Del = CarrilesDePlaza.FirstOrDefault().pla.Delegacion_Id;
                foreach (var item in Data_Delegacion)
                {
                    if (item.Id_Delegacion == Id_Del)
                    {
                        modelovirtual.Delegaciones.Add(new SQL_Delegacion
                        {
                            Id_Delegacion = item.Id_Delegacion,
                            Nom_Delegacion = item.Nom_Delegacion,
                            Num_Delegacion = item.Num_Delegacion
                        });
                    }
                }
                foreach (var item in DataPlaza)
                {
                    if (item.Id_Plaza == IdPlaza)
                    {
                        modelovirtual.Plazas.Add(new SQL_Plaza
                        {
                            Delegacion_Id = item.Delegacion_Id,
                            Id_Plaza = item.Id_Plaza,
                            Nom_Plaza = item.Nom_Plaza,
                            Num_Plaza = item.Num_Plaza
                        });
                    }
                }
                foreach (var item in CarrilesDePlaza)
                {
                    modelovirtual.Carriles.Add(new SQL_Carril
                    {
                        Id_Carril = item.car.Id_Carril,
                        Plaza_Id = item.car.Plaza_Id,
                        Num_Capufe = item.car.Num_Capufe,
                        Num_Gea = item.car.Num_Gea,
                        Num_Tramo = item.car.Num_Tramo
                    });
                }

            }
            else
            {
                foreach (var item in DataPlaza)
                {
                    if (item.Id_Plaza == IdPlaza)
                    {
                        modelovirtual.Plazas.Add(new SQL_Plaza
                        {
                            Delegacion_Id = item.Delegacion_Id,
                            Id_Plaza = item.Id_Plaza,
                            Nom_Plaza = item.Nom_Plaza,
                            Num_Plaza = item.Num_Plaza
                        });
                    }
                }
                foreach (var item in Data_Delegacion)
                {
                    if (item.Id_Delegacion == modelovirtual.Plazas.FirstOrDefault().Delegacion_Id)
                    {
                        modelovirtual.Delegaciones.Add(new SQL_Delegacion
                        {
                            Id_Delegacion = item.Id_Delegacion,
                            Nom_Delegacion = item.Nom_Delegacion,
                            Num_Delegacion = item.Num_Delegacion

                        });
                    }
                }
                ViewBag.SinCarril = "No hay carriles en la plaza " + modelovirtual.Plazas.FirstOrDefault().Nom_Plaza;

            }
            return View("Index", modelovirtual);
        }
        public ActionResult DetailsCarril(int? id, int Id_Pla)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Type_Carril type_Carril = db.Type_Carril.Find(id);
            ViewBag.SelectedPlaza = Id_Pla;
            if (type_Carril == null)
            {
                return HttpNotFound();
            }
            return View(type_Carril);
        }
        public ActionResult CreateCarril(int Id_Pla)
        {
            ViewBag.Plaza_Id = new SelectList(db.Type_Plaza, "Id_Plaza", "Nom_Plaza");
            ViewBag.SelectedPlaza = Id_Pla;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCarril([Bind(Include = "Id_Carril,Num_Gea,Num_Capufe,Num_Tramo,Plaza_Id")] Type_Carril type_Carril)
        {

            if (ModelState.IsValid)
            {
                db.Type_Carril.Add(type_Carril);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Plaza_Id = new SelectList(db.Type_Plaza, "Id_Plaza", "Num_Plaza", type_Carril.Plaza_Id);
            return View(type_Carril);
        }
        public ActionResult EditCarril(int? id, int Id_Pla)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Type_Carril type_Carril = db.Type_Carril.Find(id);
            ViewBag.SelectedPlaza = Id_Pla;
            if (type_Carril == null)
            {
                return HttpNotFound();
            }
            ViewBag.Plaza_Id = new SelectList(db.Type_Plaza, "Id_Plaza", "Nom_Plaza", type_Carril.Plaza_Id);
            return View(type_Carril);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCarril([Bind(Include = "Id_Carril,Num_Gea,Num_Capufe,Num_Tramo,Plaza_Id")] Type_Carril type_Carril)
        {

            if (ModelState.IsValid)
            {
                db.Entry(type_Carril).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Plaza_Id = new SelectList(db.Type_Plaza, "Id_Plaza", "Num_Plaza", type_Carril.Plaza_Id);
            return View(type_Carril);
        }
        public ActionResult DeleteCarril(int? id, int Id_Pla)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Type_Carril type_Carril = db.Type_Carril.Find(id);
            ViewBag.SelectedPlaza = Id_Pla;
            if (type_Carril == null)
            {
                return HttpNotFound();
            }
            return View(type_Carril);
        }
        [HttpPost, ActionName("DeleteCarril")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCarrilConfirmed(int id)
        {
            Type_Carril type_Carril = db.Type_Carril.Find(id);
            db.Type_Carril.Remove(type_Carril);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult VerOperador(int IdPlaza)
        {
            var Data_Delegacion = db.Type_Delegacion.ToList();
            var DataPlaza = db.Type_Plaza.ToList();
            var Data_Operadores = db.Type_Operadores.ToList();
            ModeloVirtual modelovirtual = new ModeloVirtual();
            modelovirtual.Delegaciones = new List<SQL_Delegacion>();
            modelovirtual.Plazas = new List<SQL_Plaza>();
            modelovirtual.Carriles = new List<SQL_Carril>();
            modelovirtual.Operadores = new List<SQL_Operadores>();
            var OperadoresDePlaza = db.Type_Plaza.Join(db.Type_Operadores, pla => pla.Id_Plaza, ope => ope.Plaza_Id, (pla, ope) => new { pla, ope }).Where(x => x.ope.Plaza_Id == IdPlaza).ToList();
            if (OperadoresDePlaza.Count != 0)
            {
                var Id_Del = OperadoresDePlaza.FirstOrDefault().pla.Delegacion_Id;
                foreach (var item in Data_Delegacion)
                {
                    if (item.Id_Delegacion == Id_Del)
                    {
                        modelovirtual.Delegaciones.Add(new SQL_Delegacion
                        {
                            Id_Delegacion = item.Id_Delegacion,
                            Nom_Delegacion = item.Nom_Delegacion,
                            Num_Delegacion = item.Num_Delegacion
                        });
                    }
                }
                foreach (var item in DataPlaza)
                {
                    if (item.Id_Plaza == IdPlaza)
                    {
                        modelovirtual.Plazas.Add(new SQL_Plaza
                        {
                            Delegacion_Id = item.Delegacion_Id,
                            Id_Plaza = item.Id_Plaza,
                            Nom_Plaza = item.Nom_Plaza,
                            Num_Plaza = item.Num_Plaza
                        });
                    }
                }
                foreach (var item in OperadoresDePlaza)
                {
                    modelovirtual.Operadores.Add(new SQL_Operadores
                    {
                        Id_Operador = item.ope.Id_Operador,
                        Plaza_Id = item.ope.Plaza_Id,
                        Nom_Operador = item.ope.Nom_Operador,
                        Num_Capufe = item.ope.Num_Capufe,
                        Num_Gea = item.ope.Num_Gea
                    });
                }

            }
            else
            {
                foreach (var item in DataPlaza)
                {
                    if (item.Id_Plaza == IdPlaza)
                    {
                        modelovirtual.Plazas.Add(new SQL_Plaza
                        {
                            Delegacion_Id = item.Delegacion_Id,
                            Id_Plaza = item.Id_Plaza,
                            Nom_Plaza = item.Nom_Plaza,
                            Num_Plaza = item.Num_Plaza
                        });
                    }
                }
                foreach (var item in Data_Delegacion)
                {
                    if (item.Id_Delegacion == modelovirtual.Plazas.FirstOrDefault().Delegacion_Id)
                    {
                        modelovirtual.Delegaciones.Add(new SQL_Delegacion
                        {
                            Id_Delegacion = item.Id_Delegacion,
                            Nom_Delegacion = item.Nom_Delegacion,
                            Num_Delegacion = item.Num_Delegacion

                        });
                    }
                }
                ViewBag.SinOperador = "No hay operadores en la plaza " + modelovirtual.Plazas.FirstOrDefault().Nom_Plaza;

            }
            return View("Index", modelovirtual);
        }
        public ActionResult DetailsOperador(int? id, int Id_Ope)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Type_Operadores type_Operadores = db.Type_Operadores.Find(id);
            ViewBag.SelectedOperador = Id_Ope;
            if (type_Operadores == null)
            {
                return HttpNotFound();
            }
            return View(type_Operadores);
        }
        public ActionResult CreateOperador(int Id_Ope)
        {
            ViewBag.SelectedOperador = Id_Ope;
            ViewBag.Plaza_Id = new SelectList(db.Type_Plaza, "Id_Plaza", "Nom_Plaza");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateOperador([Bind(Include = "Id_Operador,Num_Capufe,Num_Gea,Nom_Operador,Plaza_Id")] Type_Operadores type_Operadores)
        {
            if (ModelState.IsValid)
            {
                db.Type_Operadores.Add(type_Operadores);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Plaza_Id = new SelectList(db.Type_Plaza, "Id_Plaza", "Num_Plaza", type_Operadores.Plaza_Id);
            return View(type_Operadores);
        }
        public ActionResult EditOperador(int? id, int Id_Ope)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Type_Operadores type_Operadores = db.Type_Operadores.Find(id);
            ViewBag.SelectedOperador = Id_Ope;
            if (type_Operadores == null)
            {
                return HttpNotFound();
            }
            ViewBag.Plaza_Id = new SelectList(db.Type_Plaza, "Id_Plaza", "Nom_Plaza", type_Operadores.Plaza_Id);
            return View(type_Operadores);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditOperador([Bind(Include = "Id_Operador,Num_Capufe,Num_Gea,Nom_Operador,Plaza_Id")] Type_Operadores type_Operadores)
        {
            if (ModelState.IsValid)
            {
                db.Entry(type_Operadores).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Plaza_Id = new SelectList(db.Type_Plaza, "Id_Plaza", "Num_Plaza", type_Operadores.Plaza_Id);
            return View(type_Operadores);
        }
        public ActionResult DeleteOperador(int? id, int Id_Ope)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Type_Operadores type_Operadores = db.Type_Operadores.Find(id);
            ViewBag.SelectedOperador = Id_Ope;
            if (type_Operadores == null)
            {
                return HttpNotFound();
            }
            return View(type_Operadores);
        }
        [HttpPost, ActionName("DeleteOperador")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteOperadorConfirmed(int id)
        {
            Type_Operadores type_Operadores = db.Type_Operadores.Find(id);
            db.Type_Operadores.Remove(type_Operadores);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
