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
    //[Authorize(Roles = "Property Manager")]
    public class BuildingsController : Controller
    {
        private PropertyRentalManagementDBEntities db = new PropertyRentalManagementDBEntities();

        // GET: Buildings
        public ActionResult Index(string searchTerm, string searchType)
        {

            /*int userId = (int)Session["UserId"];

            var buildings = db.Buildings
                .Where(b => b.PropertyManagerId == userId);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                buildings = buildings
                    .Where(b => b.BuildingName.Contains(searchTerm) || b.City.Contains(searchTerm));
            }

            return View(buildings.ToList());*/

            int userId = (int)Session["UserId"];

            var buildings = db.Buildings
                .Where(b => b.PropertyManagerId == userId)
                .Include(b => b.User1); // Include Owner data

            if (!string.IsNullOrEmpty(searchTerm))
            {
                switch (searchType)
                {
                    case "BuildingName":
                        buildings = buildings.Where(b => b.BuildingName.Contains(searchTerm));
                        break;
                    case "City":
                        buildings = buildings.Where(b => b.City.Contains(searchTerm));
                        break;
                    case "OwnerId":
                        buildings = buildings.Where(b => b.OwnerId != null &&
                                                         (b.User1.FirstName + " " + b.User1.LastName).Contains(searchTerm));
                        break;
                }
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

            // Find the building record first
            Building building = db.Buildings.Find(id);
            if (building == null)
            {
                return HttpNotFound();
            }

            // Load the owner details if OwnerId is present
            if (building.OwnerId.HasValue)
            {
                building.OwnerId = db.Users.Find(building.OwnerId).UserId;
            }

            return View(building);
        }



        // GET: Buildings/Create
        public ActionResult Create()
        {
            ViewBag.OwnerList = new SelectList(
                db.Users
                    .Where(u => u.RoleId == 1)
                    .Select(u => new {
                        u.UserId,
                        FullName = u.FirstName + " " + u.LastName
                    }),
                "UserId",
                "FullName"
            );
            return View();
        }

        // POST: Buildings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
   
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Building building, HttpPostedFileBase imageUpload)
        {
            /*try
            {
                
                if (db.Buildings.Any(b => b.BuildingName == building.BuildingName))
                {
                    TempData["ErrorMessage"] = "A building with the same name already exists. Please choose a different name.";
                    return View(building);
                }

                // Restrict future dates for DateListed
                if (building.DateListed > DateTime.Today)
                {
                    ModelState.AddModelError("DateListed", "The Date Listed cannot be a future date. Please select today's date or an earlier date.");
                    return View(building); // Return early to display the error message
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

            return View(building);*/
            try
            {
                if (db.Buildings.Any(b => b.BuildingName == building.BuildingName))
                {
                    TempData["ErrorMessage"] = "A building with the same name already exists. Please choose a different name.";
                    ViewBag.OwnerId = new SelectList(db.Users.Where(u => u.RoleId == 1), "UserId", "FirstName", building.OwnerId);
                    return View(building);
                }

                if (building.DateListed > DateTime.Today)
                {
                    ModelState.AddModelError("DateListed", "The Date Listed cannot be a future date.");
                    ViewBag.OwnerId = new SelectList(db.Users.Where(u => u.RoleId == 1), "UserId", "FirstName", building.OwnerId);
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

            ViewBag.OwnerId = new SelectList(db.Users.Where(u => u.RoleId == 1), "UserId", "FirstName", building.OwnerId);
            return View(building);
        }


        // GET: Buildings/Edit/5
        public ActionResult Edit(int id)
        {

            /*int userId = (int)Session["UserId"];

            var building = db.Buildings.Find(id);
            if (building == null)
            {
                return HttpNotFound();
            }

            if (building.PropertyManagerId != userId)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            return View(building);*/


            // Populate Owner list for dropdown
            Building building = db.Buildings.Find(id);
            if (building == null)
            {
                return HttpNotFound();
            }

            // Populate the owner dropdown list
            PopulateOwnerList(building.OwnerId);

            return View(building);
        }


        // POST: Buildings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Building building, HttpPostedFileBase imageUpload)
        {
            /*try
            {
                if (ModelState.IsValid)
                {
                    // Restrict future dates for DateListed
                    if (building.DateListed > DateTime.Today)
                    {
                        ModelState.AddModelError("DateListed", "The Date Listed cannot be a future date. Please select today's date or an earlier date.");
                        return View(building); // Return early to display the error message
                    }

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

            return View(building);*/
            try
            {
                if (ModelState.IsValid)
                {
                    // Restrict future dates for DateListed
                    if (building.DateListed > DateTime.Today)
                    {
                        ModelState.AddModelError("DateListed", "The Date Listed cannot be a future date. Please select today's date or an earlier date.");
                        PopulateOwnerList(building.OwnerId);
                        return View(building);
                    }

                    int userId = (int)Session["UserId"];
                    var buildingInDb = db.Buildings.Find(building.BuildingId);

                    if (buildingInDb == null)
                    {
                        return HttpNotFound();
                    }

                    // Check if the current user is the Property Manager of the building
                    if (buildingInDb.PropertyManagerId != userId)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                    }

                    // Update properties
                    buildingInDb.BuildingName = building.BuildingName;
                    buildingInDb.Address = building.Address;
                    buildingInDb.City = building.City;
                    buildingInDb.PostalCode = building.PostalCode;
                    buildingInDb.DateListed = building.DateListed;
                    buildingInDb.OwnerId = building.OwnerId; // Update the OwnerId

                    // Handle image upload if a new file is uploaded
                    if (imageUpload != null && imageUpload.ContentLength > 0)
                    {
                        var fileName = building.BuildingName.Replace(" ", "_") + Path.GetExtension(imageUpload.FileName);
                        var path = Path.Combine(Server.MapPath("~/Content/Images"), fileName);
                        imageUpload.SaveAs(path);
                        buildingInDb.ImagePath = "/Images/" + fileName;
                    }

                    // Save changes to the database
                    db.SaveChanges();
                    TempData["Message"] = "Building updated successfully!";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the building details. " + ex.Message;
            }

            // Repopulate the owner dropdown list if ModelState is invalid
            PopulateOwnerList(building.OwnerId);
            return View(building);
        }

        // Helper method to populate the Owner list
        private void PopulateOwnerList(int? selectedOwnerId = null)
        {
            ViewBag.OwnerList = new SelectList(
                db.Users
                    .Where(u => u.Role != null && u.Role.RoleName == "Property Owner")
                    .Select(u => new
                    {
                        u.UserId,
                        FullName = u.FirstName + " " + u.LastName
                    }),
                "UserId",
                "FullName",
                selectedOwnerId
            );
        }


        // GET: Buildings/Delete/5
        public ActionResult Delete(int? id)
        {
            /*  if (id == null)
              {
                  return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
              }
              Building building = db.Buildings.Find(id);
              if (building == null)
              {
                  return HttpNotFound();
              }
              return View(building);*/
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Building building = db.Buildings.Include(b => b.OwnerId).FirstOrDefault(b => b.BuildingId == id);
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
            /*try
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

            return RedirectToAction("Index");*/
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
