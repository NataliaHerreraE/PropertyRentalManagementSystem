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
    [Authorize(Roles = "Property Manager")]
    public class ApartmentsController : Controller
    {
        private PropertyRentalManagementDBEntities db = new PropertyRentalManagementDBEntities();

        // GET: Apartments
        public ActionResult Index(string appartmentNumberStatus, string rooms, string bathrooms)
        {
            int userId = (int)Session["UserId"];
            var apartments = db.Apartments.Include(a => a.Building).Include(a => a.Status);

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
            ViewBag.BuildingId = new SelectList(db.Buildings, "BuildingId", "BuildingName");
            ViewBag.StatusId = new SelectList(db.Status, "StatusId", "StatusName");
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
                    TempData["Message"] = "Apartment created successfully!";
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
            Apartment apartment = db.Apartments.Find(id);
            if (apartment == null)
            {
                return HttpNotFound();
            }

            ViewBag.BuildingId = new SelectList(db.Buildings, "BuildingId", "BuildingName", apartment.BuildingId);
            ViewBag.StatusId = new SelectList(db.Status, "StatusId", "StatusName", apartment.StatusId);
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
                if (ModelState.IsValid)
                {
                    var apartmentInDb = db.Apartments.Find(apartment.ApartmentId);
                    if (apartmentInDb != null)
                    {
                        apartmentInDb.AppartmentNumber = apartment.AppartmentNumber;
                        apartmentInDb.Rooms = apartment.Rooms;
                        apartmentInDb.Bathrooms = apartment.Bathrooms;
                        apartmentInDb.DateListed = apartment.DateListed;
                        apartmentInDb.StatusId = apartment.StatusId;
                        apartmentInDb.BuildingId = apartment.BuildingId;

                        if (imageUpload != null && imageUpload.ContentLength > 0)
                        {
                            var fileName = apartment.AppartmentNumber + "_" + apartment.BuildingId + Path.GetExtension(imageUpload.FileName);
                            var path = Path.Combine(Server.MapPath("~/Content/Images"), fileName);
                            imageUpload.SaveAs(path);
                            apartmentInDb.ImagePath = "/Images/" + fileName;
                        }

                        db.SaveChanges();
                        TempData["Message"] = "Apartment updated successfully!";
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

            ViewBag.BuildingId = new SelectList(db.Buildings, "BuildingId", "BuildingName", apartment.BuildingId);
            ViewBag.StatusId = new SelectList(db.Status, "StatusId", "StatusName", apartment.StatusId);
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
                TempData["Message"] = "Apartment deleted successfully!";
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
