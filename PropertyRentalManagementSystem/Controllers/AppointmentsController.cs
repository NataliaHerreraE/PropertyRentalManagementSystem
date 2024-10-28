using System;
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
        public ActionResult Index()
        {
            var appointments = db.Appointments
                .Include(a => a.Status)       // Include status information
                .Include(a => a.Apartment)    // Include apartment information
                .Include(a => a.User);        // Include tenant information (User)

            // Count appointment statuses
            ViewBag.PendingCount = appointments.Count(a => a.Status.StatusName == "Pending");
            ViewBag.CompleteCount = appointments.Count(a => a.Status.StatusName == "Complete");
            ViewBag.CancelledCount = appointments.Count(a => a.Status.StatusName == "Cancelled");

            return View(appointments.ToList());
        }



        // GET: Appointments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Appointment appointment = db.Appointments.Find(id);
            if (appointment == null)
            {
                return HttpNotFound();
            }
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
