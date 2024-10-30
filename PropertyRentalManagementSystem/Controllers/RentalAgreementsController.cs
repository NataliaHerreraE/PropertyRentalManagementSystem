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
    public class RentalAgreementsController : Controller
    {
        private PropertyRentalManagementDBEntities db = new PropertyRentalManagementDBEntities();

        // GET: RentalAgreements
        public ActionResult Index(string tenantSearch, string buildingOrApartmentSearch, int? statusId)
        {
            int userId = (int)Session["UserId"];
            string roleName = (string)Session["RoleName"];

            IQueryable<RentalAgreement> agreements;

            if (roleName == "Tenant")
            {
                // Filter agreements by tenant's user ID
                agreements = db.RentalAgreements
                    .Include(ra => ra.Status)
                    .Include(ra => ra.Apartment)
                    .Include(ra => ra.Apartment.Building)
                    .Where(ra => ra.TenantId == userId);

                ViewBag.IsTenant = true;
            }
            else if (roleName == "Property Manager")
            {
                // Filter agreements by manager's buildings
                agreements = db.RentalAgreements
                    .Include(ra => ra.Status)
                    .Include(ra => ra.Apartment)
                    .Include(ra => ra.Apartment.Building)
                    .Include(ra => ra.User) // Tenant's information
                    .Where(ra => ra.Apartment.Building.PropertyManagerId == userId);

                // Apply tenant search if provided (for managers)
                if (!string.IsNullOrEmpty(tenantSearch))
                {
                    agreements = agreements.Where(ra => (ra.User.FirstName + " " + ra.User.LastName).Contains(tenantSearch));
                }

                ViewBag.IsTenant = false;
            }
            else
            {
                return RedirectToAction("Login", "Account"); // Redirect if the role is unrecognized
            }

            // Apply building or apartment search if provided (for both roles)
            if (!string.IsNullOrEmpty(buildingOrApartmentSearch))
            {
                agreements = agreements.Where(ra => ra.Apartment.Building.BuildingName.Contains(buildingOrApartmentSearch) ||
                                                     ra.Apartment.AppartmentNumber.ToString().Contains(buildingOrApartmentSearch));
            }

            // Apply status filter (for both roles)
            if (statusId.HasValue)
            {
                agreements = agreements.Where(ra => ra.StatusId == statusId.Value);
            }

            // Count active, inactive, and cancelled agreements
            ViewBag.ActiveAgreementsCount = agreements.Count(ra => ra.Status.StatusName == "Active");
            ViewBag.InactiveAgreementsCount = agreements.Count(ra => ra.Status.StatusName == "Inactive");
            ViewBag.CancelledAgreementsCount = agreements.Count(ra => ra.Status.StatusName == "Cancelled");

            // Populate the status dropdown list
            ViewBag.StatusOptions = new SelectList(db.Status
                .Where(s => s.StatusName == "Active" || s.StatusName == "Inactive" || s.StatusName == "Cancelled"),
                "StatusId", "StatusName", statusId);

            return View(agreements.ToList());
        }




        // GET: RentalAgreements/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Include the related Apartment, Building, User (Tenant), and Status information
            RentalAgreement agreement = db.RentalAgreements
                .Include(a => a.Apartment)
                .Include(a => a.Apartment.Building)
                .Include(a => a.User) // Assuming this represents the tenant
                .Include(a => a.Status) // Ensure Status is included
                .FirstOrDefault(a => a.AgreementId == id);

            if (agreement == null)
            {
                return HttpNotFound();
            }

            return View(agreement);
        }


        // GET: RentalAgreements/Create
        public ActionResult Create()
        {
            // Load Tenants (Users with RoleId = 4)
            ViewBag.TenantId = new SelectList(db.Users
                .Where(u => u.RoleId == 4) // Adjust RoleId based on your requirement for tenants
                .Select(u => new
                {
                    UserId = u.UserId,
                    FullName = u.FirstName + " " + u.LastName
                }), "UserId", "FullName");

            // Load Apartments with Building Info
            ViewBag.ApartmentId = new SelectList(db.Apartments
                .Include(a => a.Building) // Ensure Building data is loaded
                .Select(a => new
                {
                    ApartmentId = a.ApartmentId,
                    Display = a.Building.BuildingName + " - Apt " + a.AppartmentNumber
                }), "ApartmentId", "Display");

            return View();
        }

        // POST: RentalAgreements/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RentalAgreement agreement)
        {
            if (ModelState.IsValid)
            {
                // Set the Status based on EndDate
                var today = DateTime.Today;
                agreement.StatusId = agreement.EndDate >= today
                    ? db.Status.FirstOrDefault(s => s.StatusName == "Active").StatusId
                    : db.Status.FirstOrDefault(s => s.StatusName == "Inactive").StatusId;

                db.RentalAgreements.Add(agreement);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Rental Agreement created successfully.";
                return RedirectToAction("Index");
            }

            // Reload necessary data if the ModelState is not valid
            ViewBag.TenantId = new SelectList(db.Users
                .Where(u => u.RoleId == 4)
                .Select(u => new
                {
                    UserId = u.UserId,
                    FullName = u.FirstName + " " + u.LastName
                }), "UserId", "FullName");

            ViewBag.ApartmentId = new SelectList(db.Apartments
                .Include(a => a.Building)
                .Select(a => new
                {
                    ApartmentId = a.ApartmentId,
                    Display = a.Building.BuildingName + " - Apt " + a.AppartmentNumber
                }), "ApartmentId", "Display");

            TempData["ErrorMessage"] = "There was an error creating the rental agreement.";
            return View(agreement);
        }

        public ActionResult Edit(int id)
        {
            var rentalAgreement = db.RentalAgreements
                                    .Include(r => r.Apartment)
                                    .Include(r => r.User)
                                    .FirstOrDefault(r => r.AgreementId == id);

            if (rentalAgreement == null)
            {
                return HttpNotFound();
            }

            return View(rentalAgreement);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(RentalAgreement rentalAgreement)
        {
            if (ModelState.IsValid)
            {
                // Fetch the current agreement from the database
                var currentAgreement = db.RentalAgreements.Include(a => a.Status).FirstOrDefault(a => a.AgreementId == rentalAgreement.AgreementId);
                if (currentAgreement == null)
                {
                    TempData["ErrorMessage"] = "Rental Agreement not found.";
                    return RedirectToAction("Index");
                }

                // Update the EndDate and Status based on the new EndDate
                currentAgreement.EndDate = rentalAgreement.EndDate;

                // Check the EndDate to update the Status
                if (currentAgreement.EndDate >= DateTime.Today)
                {
                    currentAgreement.StatusId = db.Status.FirstOrDefault(s => s.StatusName == "Active")?.StatusId ?? currentAgreement.StatusId;
                }
                else
                {
                    currentAgreement.StatusId = db.Status.FirstOrDefault(s => s.StatusName == "InActive")?.StatusId ?? currentAgreement.StatusId;
                }

                db.SaveChanges();
                TempData["SuccessMessage"] = "Rental Agreement updated successfully!";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "There was an issue updating the rental agreement. Please check your inputs.";
            return View(rentalAgreement);
        }




        public ActionResult UpdateStatusByDate(int id)
        {
            var agreement = db.RentalAgreements.Find(id);
            if (agreement == null)
            {
                TempData["ErrorMessage"] = "Rental Agreement not found.";
                return RedirectToAction("Index");
            }

            if (agreement.EndDate >= DateTime.Today)
            {
                agreement.StatusId = db.Status.FirstOrDefault(s => s.StatusName == "Active").StatusId;
            }
            else
            {
                agreement.StatusId = db.Status.FirstOrDefault(s => s.StatusName == "Inactive").StatusId;
            }

            db.SaveChanges();
            TempData["SuccessMessage"] = "Status updated based on date.";
            return RedirectToAction("Edit", new { id = agreement.AgreementId });
        }

        public ActionResult CancelAgreement(int id)
        {
            var agreement = db.RentalAgreements.Find(id);
            if (agreement == null)
            {
                TempData["ErrorMessage"] = "Rental Agreement not found.";
                return RedirectToAction("Index");
            }

            agreement.StatusId = db.Status.FirstOrDefault(s => s.StatusName == "Cancelled").StatusId;

            db.SaveChanges();
            TempData["SuccessMessage"] = "Rental Agreement cancelled successfully.";
            return RedirectToAction("Edit", new { id = agreement.AgreementId });
        }




        // GET: RentalAgreements/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Fetch the RentalAgreement with related Apartment, Building, and Tenant details
            RentalAgreement agreement = db.RentalAgreements
                .Include(a => a.Apartment)
                .Include(a => a.Apartment.Building)
                .Include(a => a.User) // Assuming this represents the tenant
                .FirstOrDefault(a => a.AgreementId == id);

            if (agreement == null)
            {
                TempData["ErrorMessage"] = "Rental Agreement not found.";
                return RedirectToAction("Index");
            }

            return View(agreement);
        }

        // POST: RentalAgreements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            RentalAgreement agreement = db.RentalAgreements.Find(id);
            if (agreement == null)
            {
                TempData["ErrorMessage"] = "Rental Agreement not found.";
                return RedirectToAction("Index");
            }

            // Check if there are any payments associated with this rental agreement
            bool hasPayments = db.Payments.Any(p => p.AgreementId == id);
            if (hasPayments)
            {
                TempData["ErrorMessage"] = "An existing agreement with payments cannot be deleted. You can cancel this agreement if you want to discontinue it.";
                return RedirectToAction("Index");
            }

            db.RentalAgreements.Remove(agreement);
            db.SaveChanges();
            TempData["SuccessMessage"] = "Rental Agreement deleted successfully.";
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
