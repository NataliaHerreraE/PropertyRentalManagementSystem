using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PropertyRentalManagementSystem.Models;

namespace PropertyRentalManagementSystem.Controllers
{
    //[Authorize(Roles = "Property Manager")]
    public class ApartmentsController : Controller
    {
        private PropertyRentalManagementDBEntities db = new PropertyRentalManagementDBEntities();

        // GET: Apartments
        public ActionResult Index(string appartmentNumberStatus, string rooms, string bathrooms, string price)
        {
            int userId = (int)Session["UserId"];
            var apartments = db.Apartments.Include(a => a.Building).Include(a => a.Status);

            ViewBag.AvailableApartmentCount = apartments.Count(a => a.Status.StatusName == "Available");
            ViewBag.RentedApartmentCount = apartments.Count(a => a.Status.StatusName == "Rented");


            // Filter by AppartmentNumber or Status if search term is provided
            if (!string.IsNullOrEmpty(appartmentNumberStatus))
            {
                apartments = apartments.Where(a =>
                    a.AppartmentNumber.Contains(appartmentNumberStatus) ||
                    a.Status.StatusName.Contains(appartmentNumberStatus));
            }

            // Filter by Rooms if search term is provided
            if (!string.IsNullOrEmpty(rooms))
            {
                if (int.TryParse(rooms, out int roomCount))
                {
                    apartments = apartments.Where(a => a.Rooms == roomCount);
                }
                else
                {
                    TempData["ErrorMessage"] = "Please enter a valid number for Rooms.";
                }
            }

            // Filter by Bathrooms if search term is provided
            if (!string.IsNullOrEmpty(bathrooms))
            {
                if (int.TryParse(bathrooms, out int bathroomCount))
                {
                    apartments = apartments.Where(a => a.Bathrooms == bathroomCount);
                }
                else
                {
                    TempData["ErrorMessage"] = "Please enter a valid number for Bathrooms.";
                }
            }

            // Filter by Price if search term is provided
            if (!string.IsNullOrEmpty(price))
            {
                if (decimal.TryParse(price, out decimal priceValue))
                {
                    apartments = apartments.Where(a => a.Price <= priceValue);
                }
                else
                {
                    TempData["ErrorMessage"] = "Please enter a valid price.";
                }
            }

            return View(apartments.ToList());
        }

        // GET: Apartments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Apartment apartment = db.Apartments.Find(id);
            if (apartment == null)
            {
                return HttpNotFound();
            }
            return View(apartment);
        }

        // GET: Apartments/Create
        public ActionResult Create()
        {
            // Only show 'Available' and 'Rented' statuses in the dropdown
            var statusList = db.Status
                               .Where(s => s.StatusName == "Available" || s.StatusName == "Rented")
                               .ToList();
            ViewBag.StatusId = new SelectList(statusList, "StatusId", "StatusName");
            ViewBag.BuildingId = new SelectList(db.Buildings, "BuildingId", "BuildingName");

            return View();
        }

        // POST: Apartments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Apartment apartment, HttpPostedFileBase imageUpload)
        {
            try
            {
                if (apartment.DateListed > DateTime.Today)
                {
                    ModelState.AddModelError("DateListed", "The Date Listed cannot be a future date. Please select today's date or an earlier date.");
                }

                // Check for duplicate apartment number within the same building
                if (db.Apartments.Any(a => a.BuildingId == apartment.BuildingId && a.AppartmentNumber == apartment.AppartmentNumber))
                {
                    TempData["ErrorMessage"] = "An apartment with the same number already exists in this building.";
                    ViewBag.BuildingId = new SelectList(db.Buildings, "BuildingId", "BuildingName", apartment.BuildingId);
                    ViewBag.StatusId = new SelectList(db.Status, "StatusId", "StatusName", apartment.StatusId);
                    return View(apartment);
                }

                if (ModelState.IsValid)
                {
                    // Image upload handling
                    if (imageUpload != null && imageUpload.ContentLength > 0)
                    {
                        var fileName = apartment.AppartmentNumber + "_" + apartment.BuildingId + Path.GetExtension(imageUpload.FileName);
                        var path = Path.Combine(Server.MapPath("~/Content/Images"), fileName);
                        imageUpload.SaveAs(path);
                        apartment.ImagePath = "/Images/" + fileName;
                    }
                    else
                    {
                        apartment.ImagePath = "/Images/default.jpg";
                    }

                    db.Apartments.Add(apartment);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Apartment created successfully!";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while creating the apartment. " + ex.Message;
            }

            ViewBag.BuildingId = new SelectList(db.Buildings, "BuildingId", "BuildingName", apartment.BuildingId);
            ViewBag.StatusId = new SelectList(db.Status, "StatusId", "StatusName", apartment.StatusId);
            return View(apartment);
        }


        // GET: Apartments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var apartment = db.Apartments
                              .Include(a => a.Status)
                              .Include(a => a.Building)
                              .FirstOrDefault(a => a.ApartmentId == id);

            if (apartment == null)
            {
                return HttpNotFound();
            }

            // Status dropdown with the current status selected
            var statusList = db.Status
                               .Where(s => s.StatusName == "Available" || s.StatusName == "Rented")
                               .ToList();
            ViewBag.StatusId = new SelectList(statusList, "StatusId", "StatusName", apartment.StatusId);

            // Building dropdown with the current building selected
            ViewBag.BuildingId = new SelectList(db.Buildings, "BuildingId", "BuildingName", apartment.BuildingId);

            return View(apartment);
        }


        // POST: Apartments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Apartment apartment, HttpPostedFileBase imageUpload)
        {
            try
            {
                // Check if the date is in the future
                if (apartment.DateListed > DateTime.Today)
                {
                    ModelState.AddModelError("DateListed", "The Date Listed cannot be a future date. Please select today's date or an earlier date.");
                }

                if (ModelState.IsValid)
                {
                    var apartmentInDb = db.Apartments.Find(apartment.ApartmentId);
                    if (apartmentInDb != null)
                    {
                        apartmentInDb.AppartmentNumber = apartment.AppartmentNumber;
                        apartmentInDb.Rooms = apartment.Rooms;
                        apartmentInDb.Bathrooms = apartment.Bathrooms;
                        apartmentInDb.Price = apartment.Price;
                        apartmentInDb.DateListed = apartment.DateListed;
                        apartmentInDb.StatusId = apartment.StatusId;
                        apartmentInDb.BuildingId = apartment.BuildingId;

                        // Handle image upload if a new file is uploaded
                        if (imageUpload != null && imageUpload.ContentLength > 0)
                        {
                            var fileName = apartment.AppartmentNumber + "_" + apartment.BuildingId + Path.GetExtension(imageUpload.FileName);
                            var path = Path.Combine(Server.MapPath("~/Content/Images"), fileName);
                            imageUpload.SaveAs(path);
                            apartmentInDb.ImagePath = "/Images/" + fileName;
                        }

                        db.SaveChanges();
                        TempData["SuccessMessage"] = "Apartment updated successfully!";
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
                TempData["ErrorMessage"] = "An error occurred while updating the apartment. " + ex.Message;
            }

            // Reload dropdowns for Status and Building in case of an error
            var statusList = db.Status.Where(s => s.StatusName == "Available" || s.StatusName == "Rented").ToList();
            ViewBag.StatusId = new SelectList(statusList, "StatusId", "StatusName", apartment.StatusId);
            ViewBag.BuildingId = new SelectList(db.Buildings, "BuildingId", "BuildingName", apartment.BuildingId);

            return View(apartment);
        }



        // GET: Apartments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Apartment apartment = db.Apartments.Find(id);
            if (apartment == null)
            {
                return HttpNotFound();
            }
            return View(apartment);
        }

        // POST: Apartments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Apartment apartment = db.Apartments.Find(id);
                if (apartment == null)
                {
                    TempData["ErrorMessage"] = "Apartment not found.";
                    return RedirectToAction("Index");
                }

                db.Apartments.Remove(apartment);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Apartment deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the apartment. " + ex.Message;
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
