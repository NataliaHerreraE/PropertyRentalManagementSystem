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
    public class BuildingsController : Controller
    {
        private PropertyRentalManagementDBEntities db = new PropertyRentalManagementDBEntities();

        // GET: Buildings
        public ActionResult Index(string searchTerm)
        {
            // Check if UserId is present in the session
            if (Session["UserId"] == null)
            {
                // Redirect to the login page if the session has expired or user is not logged in
                return RedirectToAction("Login", "Account");
            }

            // Get the current user ID
            int userId = (int)Session["UserId"];

            // Fetch buildings associated with the current property manager
            var buildings = db.Buildings
                .Where(b => b.PropertyManagerId == userId);

            // If there is a search term, filter by BuildingName or City
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
            //ViewBag.PropertyManagerId = new SelectList(db.Users, "UserId", "FirstName");
            return View();
        }

        // POST: Buildings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Buildings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Building building, HttpPostedFileBase imageUpload)
        {
            try
            {
                // Check if a building with the same name already exists
                if (db.Buildings.Any(b => b.BuildingName == building.BuildingName))
                {
                    TempData["ErrorMessage"] = "A building with the same name already exists. Please choose a different name.";
                    return View(building);
                }

                if (ModelState.IsValid)
                {
                    // Get the current user ID from the session
                    int userId = (int)Session["UserId"];
                    building.PropertyManagerId = userId;

                    // Handle image upload if provided
                    if (imageUpload != null && imageUpload.ContentLength > 0)
                    {
                        // Format the file name using the BuildingName (replacing spaces with underscores)
                        var fileName = building.BuildingName.Replace(" ", "_") + Path.GetExtension(imageUpload.FileName);
                        var path = Path.Combine(Server.MapPath("~/Content/Images"), fileName);

                        // Save the new image
                        imageUpload.SaveAs(path);

                        // Update the ImagePath with the relative path
                        building.ImagePath = "/Images/" + fileName;
                    }
                    else
                    {
                        // Optional: Set a default image path if no image is uploaded
                        building.ImagePath = "/Images/default.jpg";
                    }

                    // Add the building to the database
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
            // Check if the user is logged in
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account"); // Redirect to login if UserId is not in session
            }

            // Get the current user ID from the session
            int userId = (int)Session["UserId"];

            var building = db.Buildings.Find(id);
            if (building == null)
            {
                return HttpNotFound();
            }

            // Check if the current user is the Property Manager for this building
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
                    // Get the current user ID from the session
                    int userId = (int)Session["UserId"];

                    // Load existing building from the database
                    var buildingInDb = db.Buildings.Find(building.BuildingId);
                    if (buildingInDb != null)
                    {
                        // Ensure that the user has permission to edit this building
                        if (buildingInDb.PropertyManagerId != userId)
                        {
                            return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                        }

                        // Update properties manually
                        buildingInDb.BuildingName = building.BuildingName;
                        buildingInDb.Address = building.Address;
                        buildingInDb.City = building.City;
                        buildingInDb.PostalCode = building.PostalCode;
                        buildingInDb.DateListed = building.DateListed;

                        // If a new image is uploaded, update the ImagePath
                        if (imageUpload != null && imageUpload.ContentLength > 0)
                        {
                            // Format the file name using the BuildingName (replacing spaces with underscores)
                            var fileName = building.BuildingName.Replace(" ", "_") + Path.GetExtension(imageUpload.FileName);
                            var path = Path.Combine(Server.MapPath("~/Content/Images"), fileName);

                            // Save the new image
                            imageUpload.SaveAs(path);

                            // Update the ImagePath with the relative path
                            buildingInDb.ImagePath = "/Images/" + fileName;
                        }
                        // If no new image was uploaded, keep the existing ImagePath
                        else
                        {
                            TempData["Message"] = "Building updated successfully!";
                            building.ImagePath = buildingInDb.ImagePath;
                        }

                        // Save changes
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
            Building building = db.Buildings.Find(id);
            db.Buildings.Remove(building);
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
    }
}
