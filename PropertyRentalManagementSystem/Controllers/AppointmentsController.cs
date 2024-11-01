﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PropertyRentalManagementSystem.Models;

namespace PropertyRentalManagementSystem.Controllers
{
    //[Authorize(Roles = "Property Manager")]
    public class AppointmentsController : Controller
    {
        private PropertyRentalManagementDBEntities db = new PropertyRentalManagementDBEntities();

        // GET: Appointments
        public ActionResult Index(string tenantSearch, DateTime? dateSearch, int? statusId, int? buildingId, int? apartmentId)
        {
            int userId = (int)Session["UserId"];
            string roleName = (string)Session["RoleName"];

            IQueryable<Appointment> appointments;

            if (roleName == "Tenant")
            {
                // Get appointments for the tenant, including the manager's information via the Building table
                appointments = db.Appointments
                    .Include(a => a.Status)
                    .Include(a => a.Apartment)
                    .Include(a => a.Apartment.Building)
                    .Include(a => a.Apartment.Building.User) // Include the User entity to access manager details
                    .Where(a => a.TenantId == userId);

                ViewBag.IsTenant = true;

                // Count appointment statuses for the tenant
                ViewBag.PendingCount = appointments.Count(a => a.Status.StatusName == "Pending");
                ViewBag.CompleteCount = appointments.Count(a => a.Status.StatusName == "Complete");
                ViewBag.CancelledCount = appointments.Count(a => a.Status.StatusName == "Cancelled");
            }
            else if (roleName == "Property Manager")
            {
                // Get appointments managed by the current property manager
                appointments = db.Appointments
                    .Include(a => a.Status)
                    .Include(a => a.Apartment)
                    .Include(a => a.Apartment.Building)
                    .Include(a => a.User) // Include the Tenant's information
                    .Where(a => a.Apartment.Building.PropertyManagerId == userId);

                ViewBag.IsTenant = false;

                // Count appointment statuses for the manager
                ViewBag.PendingCount = appointments.Count(a => a.Status.StatusName == "Pending");
                ViewBag.CompleteCount = appointments.Count(a => a.Status.StatusName == "Complete");
                ViewBag.CancelledCount = appointments.Count(a => a.Status.StatusName == "Cancelled");
            }
            else
            {
                return RedirectToAction("Login", "Account"); // Redirect if the role is unrecognized
            }

            // Apply search filters for both roles
            if (dateSearch.HasValue)
            {
                appointments = appointments.Where(a => DbFunctions.TruncateTime(a.Date) == DbFunctions.TruncateTime(dateSearch.Value));
            }

            if (statusId.HasValue)
            {
                appointments = appointments.Where(a => a.StatusId == statusId.Value);
            }

            if (buildingId.HasValue)
            {
                appointments = appointments.Where(a => a.Apartment.BuildingId == buildingId.Value);
            }

            if (apartmentId.HasValue)
            {
                appointments = appointments.Where(a => a.ApartmentId == apartmentId.Value);
            }

            // Populate dropdown lists for filtering
            ViewBag.StatusId = new SelectList(db.Status
                .Where(s => s.StatusName == "Pending" || s.StatusName == "Complete" || s.StatusName == "Cancelled"),
                "StatusId", "StatusName", statusId);

            if (roleName == "Property Manager")
            {
                ViewBag.BuildingId = new SelectList(db.Buildings.Where(b => b.PropertyManagerId == userId), "BuildingId", "BuildingName", buildingId);

                // Populate ApartmentId dropdown with all apartments managed by the current user
                ViewBag.ApartmentId = new SelectList(db.Apartments
                    .Where(a => a.Building.PropertyManagerId == userId)
                    .Select(a => new
                    {
                        a.ApartmentId,
                        DisplayText = a.AppartmentNumber + " - " + a.Building.BuildingName
                    }), "ApartmentId", "DisplayText", apartmentId);
            }
            else if (roleName == "Tenant")
            {
                // For tenants, populate only buildings and apartments relevant to their appointments
                ViewBag.BuildingId = new SelectList(appointments
                    .Select(a => a.Apartment.Building).Distinct(), "BuildingId", "BuildingName", buildingId);

                ViewBag.ApartmentId = new SelectList(appointments
                    .Select(a => new
                    {
                        a.Apartment.ApartmentId,
                        DisplayText = a.Apartment.AppartmentNumber + " - " + a.Apartment.Building.BuildingName
                    }).Distinct(), "ApartmentId", "DisplayText", apartmentId);
            }

            return View(appointments.ToList());
        }







        // GET: Appointments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Ensure the `User` (manager) entity is included along with the building and apartment information
            Appointment appointment = db.Appointments
                .Include(a => a.Status)
                .Include(a => a.Apartment)
                .Include(a => a.Apartment.Building)
                .Include(a => a.Apartment.Building.User) // Ensure manager details are loaded
                .FirstOrDefault(a => a.AppointmentId == id);

            if (appointment == null)
            {
                return HttpNotFound();
            }

            // Set the `IsTenant` flag for conditional display in the view
            ViewBag.IsTenant = (string)Session["RoleName"] == "Tenant";

            return View(appointment);
        }


        // GET: Appointments/Create
        public ActionResult Create()
        {
            int userId = (int)Session["UserId"];

            // Populate apartments dropdown with only apartments managed by the current manager
            ViewBag.ApartmentId = new SelectList(db.Apartments
                .Where(a => a.Building.PropertyManagerId == userId), "ApartmentId", "AppartmentNumber");

            // Populate tenants dropdown with all tenants
            ViewBag.TenantId = new SelectList(db.Users
                .Where(u => u.RoleId == 4) // Assuming RoleId 4 is for tenants
                .Select(u => new { u.UserId, FullName = u.FirstName + " " + u.LastName }),
                "UserId", "FullName");

            // Populate status dropdown with "Pending", "Cancelled", and "Complete"
            ViewBag.StatusId = new SelectList(db.Status
                .Where(s => s.StatusName == "Pending" || s.StatusName == "Cancelled" || s.StatusName == "Complete"),
                "StatusId", "StatusName");

            return View();
        }



        // POST: Appointments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Appointment appointment)
        {
            try
            {
                // Validate that the date is not in the past
                if (appointment.Date < DateTime.Today)
                {
                    ModelState.AddModelError("Date", "The appointment date cannot be in the past. Please select today or a future date.");
                }

                if (ModelState.IsValid)
                {
                    db.Appointments.Add(appointment);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Appointment created successfully!";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while creating the appointment. " + ex.Message;
            }

            int userId = (int)Session["UserId"];

            // Reload dropdowns in case of validation errors
            ViewBag.ApartmentId = new SelectList(db.Apartments
                .Where(a => a.Building.PropertyManagerId == userId), "ApartmentId", "AppartmentNumber", appointment.ApartmentId);

            ViewBag.TenantId = new SelectList(db.Users
                .Where(u => u.RoleId == 4), "UserId", "FullName", appointment.TenantId);

            ViewBag.StatusId = new SelectList(db.Status
                .Where(s => s.StatusName == "Pending" || s.StatusName == "Cancelled" || s.StatusName == "Complete"),
                "StatusId", "StatusName", appointment.StatusId);

            return View(appointment);
        }

        // GET: Appointments/Edit/5
        public ActionResult Edit(int id)
        {
            var appointment = db.Appointments
                                .Include(a => a.Apartment)
                                .Include(a => a.User)
                                .Include(a => a.Status)
                                .FirstOrDefault(a => a.AppointmentId == id);


            if (appointment == null)
            {
                return HttpNotFound();
            }

            int userId = (int)Session["UserId"];
            var tenantRoleId = 4;

            ViewBag.ApartmentId = new SelectList(db.Apartments
                                                 .Where(a => a.Building.PropertyManagerId == userId),
                                                 "ApartmentId",
                                                 "AppartmentNumber",
                                                 appointment.ApartmentId); // Preselect current ApartmentId

            ViewBag.TenantId = new SelectList(db.Users
                                              .Where(u => u.RoleId == tenantRoleId)
                                              .Select(u => new
                                              {
                                                  u.UserId,
                                                  FullName = u.FirstName + " " + u.LastName
                                              }),
                                              "UserId",
                                              "FullName",
                                              appointment.TenantId); // Preselect current TenantId

            ViewBag.StatusId = new SelectList(db.Status
                                              .Where(s => s.StatusName == "Pending" || s.StatusName == "Cancelled" || s.StatusName == "Complete"),
                                              "StatusId",
                                              "StatusName",
                                              appointment.StatusId); // Preselect current StatusId

            return View(appointment);
        }






        // POST: Appointments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Appointment appointment)
        {
            try
            {
                bool isValid = true;

                // Validate the date based on status
                if (appointment.StatusId == db.Status.FirstOrDefault(s => s.StatusName == "Pending").StatusId && appointment.Date <= DateTime.Now)
                {
                    ModelState.AddModelError("Date", "The appointment date must be in the future to set the status as Pending.");
                    TempData["ErrorMessage"] = "The appointment date must be in the future to set the status as Pending.";
                    isValid = false;
                }

                if (appointment.StatusId == db.Status.FirstOrDefault(s => s.StatusName == "Cancelled").StatusId && appointment.Date <= DateTime.Now)
                {
                    ModelState.AddModelError("Date", "The appointment date must be in the future to cancel.");
                    TempData["ErrorMessage"] = "The appointment date must be in the future to cancel.";
                    isValid = false;
                }

                if (appointment.StatusId == db.Status.FirstOrDefault(s => s.StatusName == "Complete").StatusId && appointment.Date > DateTime.Now)
                {
                    ModelState.AddModelError("Date", "The appointment date must be in the past to mark as Complete.");
                    TempData["ErrorMessage"] = "The appointment date must be in the past to mark as Complete.";
                    isValid = false;
                }

                if (isValid && ModelState.IsValid)
                {
                    var appointmentInDb = db.Appointments.Find(appointment.AppointmentId);
                    if (appointmentInDb == null)
                    {
                        TempData["ErrorMessage"] = "Appointment not found.";
                        return RedirectToAction("Index");
                    }

                    // Update appointment
                    appointmentInDb.ApartmentId = appointment.ApartmentId;
                    appointmentInDb.TenantId = appointment.TenantId;
                    appointmentInDb.Date = appointment.Date;
                    appointmentInDb.StatusId = appointment.StatusId;

                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Appointment updated successfully!";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the appointment. " + ex.Message;
            }

            // Reload dropdowns and model details in case of validation errors
            int userId = (int)Session["UserId"];

            ViewBag.ApartmentId = new SelectList(db.Apartments
                                                 .Where(a => a.Building.PropertyManagerId == userId),
                                                 "ApartmentId", "AppartmentNumber", appointment.ApartmentId);

            ViewBag.TenantId = new SelectList(db.Users
                                              .Where(u => u.RoleId == 4)
                                              .Select(u => new { u.UserId, FullName = u.FirstName + " " + u.LastName }),
                                              "UserId", "FullName", appointment.TenantId);

            ViewBag.StatusId = new SelectList(db.Status
                                              .Where(s => s.StatusName == "Pending" || s.StatusName == "Cancelled" || s.StatusName == "Complete"),
                                              "StatusId", "StatusName", appointment.StatusId);

            return View(appointment);
        }





        // GET: Appointments/Delete/5
        public ActionResult Delete(int id)
        {
            var appointment = db.Appointments
                .Include(a => a.Apartment)
                .Include(a => a.Status)
                .Include(a => a.User)
                .FirstOrDefault(a => a.AppointmentId == id);

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction("Index");
            }

            return View(appointment);
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                var appointment = db.Appointments.Find(id);
                if (appointment != null)
                {
                    db.Appointments.Remove(appointment);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Appointment deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Appointment not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the appointment. " + ex.Message;
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
