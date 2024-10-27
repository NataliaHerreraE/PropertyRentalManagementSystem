using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using PropertyRentalManagementSystem.Models;

namespace PropertyRentalManagementSystem.Controllers
{
    [Authorize(Roles = "Property Manager")]
    public class BuildingsController : Controller
    {
        private PropertyRentalManagementDBEntities db = new PropertyRentalManagementDBEntities();

        // GET: Buildings
        public ActionResult Index(string searchTerm)
        {
            /*
            if (Session["UserId"] == null)
            {
                
                return RedirectToAction("Login", "Account");
            }*/
            int userId = (int)Session["UserId"];

            var buildings = db.Buildings
                .Where(b => b.PropertyManagerId == userId);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                buildings = buildings
                    .Where(b => b.BuildingName.Contains(searchTerm) || b.City.Contains(searchTerm));
            }

            return View(buildings.ToList());
        }



        // GET: Buildings/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Building building = db.Buildings.Find(id);
            if (building == null)
            {
                return HttpNotFound();
            }
            return View(building);
        }

        // GET: Buildings/Create
        public ActionResult Create()
        {
            
            return View();
        }

        // POST: Buildings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
   
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Building building, HttpPostedFileBase imageUpload)
        {
            try
            {
                
                if (db.Buildings.Any(b => b.BuildingName == building.BuildingName))
                {
                    TempData["ErrorMessage"] = "A building with the same name already exists. Please choose a different name.";
                    return View(building);
                }

                if (ModelState.IsValid)
                {
                    
                    int userId = (int)Session["UserId"];
                    building.PropertyManagerId = userId;

                   
                    if (imageUpload != null && imageUpload.ContentLength > 0)
                    {
                        
                        var fileName = building.BuildingName.Replace(" ", "_") + Path.GetExtension(imageUpload.FileName);
                        var path = Path.Combine(Server.MapPath("~/Content/Images"), fileName);

                      
                        imageUpload.SaveAs(path);

                        
                        building.ImagePath = "/Images/" + fileName;
                    }
                    else
                    {
                       
                        building.ImagePath = "/Images/default.jpg";
                    }

            
                    db.Buildings.Add(building);
                    db.SaveChanges();

                    TempData["Message"] = "Building created successfully!";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while creating the building. " + ex.Message;
            }

            return View(building);
        }


        // GET: Buildings/Edit/5
        public ActionResult Edit(int id)
        {

            int userId = (int)Session["UserId"];

            var building = db.Buildings.Find(id);
            if (building == null)
            {
                return HttpNotFound();
            }

            if (building.PropertyManagerId != userId)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            return View(building);
        }


        // POST: Buildings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Building building, HttpPostedFileBase imageUpload)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    
                    int userId = (int)Session["UserId"];

                    var buildingInDb = db.Buildings.Find(building.BuildingId);
                    if (buildingInDb != null)
                    {
                       
                        if (buildingInDb.PropertyManagerId != userId)
                        {
                            return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                        }

                        buildingInDb.BuildingName = building.BuildingName;
                        buildingInDb.Address = building.Address;
                        buildingInDb.City = building.City;
                        buildingInDb.PostalCode = building.PostalCode;
                        buildingInDb.DateListed = building.DateListed;

                       
                        if (imageUpload != null && imageUpload.ContentLength > 0)
                        {
                           
                            var fileName = building.BuildingName.Replace(" ", "_") + Path.GetExtension(imageUpload.FileName);
                            var path = Path.Combine(Server.MapPath("~/Content/Images"), fileName);

                           
                            imageUpload.SaveAs(path);

                           
                            buildingInDb.ImagePath = "/Images/" + fileName;
                        }
                  
                        else
                        {
                            TempData["Message"] = "Building updated successfully!";
                            building.ImagePath = buildingInDb.ImagePath;
                        }

                     
                        db.SaveChanges();
                        TempData["Message"] = "Building updated successfully!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return HttpNotFound();
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the building details. " + ex.Message;
            
            }

            return View(building);
        }


        // GET: Buildings/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Building building = db.Buildings.Find(id);
            if (building == null)
            {
                return HttpNotFound();
            }
            return View(building);
        }

        // POST: Buildings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Building building = db.Buildings.Find(id);
                if (building == null)
                {
                    TempData["ErrorMessage"] = "Building not found.";
                    return RedirectToAction("Index");
                }

                db.Buildings.Remove(building);
                db.SaveChanges();

                TempData["Message"] = "Building deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the building. " + ex.Message;
            }

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
    }
}
