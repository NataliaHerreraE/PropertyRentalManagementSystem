using PropertyRentalManagementSystem.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace PropertyRentalManagementSystem.Controllers
{
    public class TenantController : Controller
    {
        private PropertyRentalManagementDBEntities db = new PropertyRentalManagementDBEntities();

        // GET: Tenant/Index
        public ActionResult Index(string searchTerm)
        {

            if (Session["UserId"] == null)
            {

                return RedirectToAction("Login", "Account");
            }
            int userId = (int)Session["UserId"];

            var tenants = db.Users.Where(u => u.Role.RoleName == "Tenant");

            if (!string.IsNullOrEmpty(searchTerm))
            {
                tenants = tenants.Where(u => u.FirstName.Contains(searchTerm) || u.LastName.Contains(searchTerm));
            }

            return View(tenants.ToList());
        }

        public ActionResult ListTenants()
        {
            var tenant = db.Users.Where(u => u.Role.RoleName == "Tenant").ToList();
            return View(tenant);
        }


        // GET: Tenant/Edit/{id}
        public ActionResult Edit(int id)
        {
            var tenant = db.Users.Find(id);
            if (tenant == null || tenant.Role.RoleName != "Tenant")
            {
                return HttpNotFound();
            }
            return View(tenant);
        }

        // POST: Tenant/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(User model)
        {
            // Validate if all fields are provided (not null or empty)
            if (string.IsNullOrWhiteSpace(model.FirstName) ||
                string.IsNullOrWhiteSpace(model.LastName) ||
                string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.Phone))
            {
                TempData["ErrorMessage"] = "Error: All fields must be filled in.";
                return View(model);  // Return the form with error message
            }

            if (ModelState.IsValid)
            {
                var tenant = db.Users.Find(model.UserId);

                if (tenant == null)
                {
                    TempData["ErrorMessage"] = "Error: Tenant not found.";
                    return HttpNotFound();
                }

                // Check if the email has changed
                if (tenant.Email != model.Email)
                {
                    // Check if the new email is already used by any other user, regardless of role
                    var existingUserWithSameEmail = db.Users.FirstOrDefault(u => u.Email == model.Email && u.UserId != model.UserId);
                    if (existingUserWithSameEmail != null)
                    {
                        TempData["ErrorMessage"] = "Error: Email is already in use.";
                        return View(model);
                    }

                    tenant.Email = model.Email;
                }

                tenant.FirstName = model.FirstName;
                tenant.LastName = model.LastName;
                tenant.Phone = model.Phone;

                try
                {
                    // Mark the entity as modified
                    db.Entry(tenant).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Message"] = "Tenant updated successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Error: Unable to save changes to the database.";
                    return View(model);  // Re-render the form with error message
                }
            }

            TempData["ErrorMessage"] = "Error: Unable to update Tenant. Please correct the errors and try again.";
            return View(model);  // Re-render the form with validation errors
        }

        // GET: Tenant/Delete/{id}
        public ActionResult Delete(int id)
        {
            var tenant = db.Users.Find(id);
            if (tenant == null || tenant.Role.RoleName != "Tenant")
            {
                TempData["ErrorMessage"] = "Error: Tenant not found.";
                return RedirectToAction("Index");
            }

            // Check if the tenant is linked to an apartment or any other related record
            var tenantHasApartment = db.RentalAgreements.Any(ra => ra.TenantId == id);
            if (tenantHasApartment)
            {
                TempData["ErrorMessage"] = "You cannot delete this Tenant because they are linked to one or more apartments.";
                return RedirectToAction("Index");
            }

            try
            {
                // Proceed with the deletion if no dependencies are found
                db.Users.Remove(tenant);
                db.SaveChanges();
                TempData["Message"] = "Tenant deleted successfully!";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Error: Unable to delete the tenant. It may be linked to other records in the system.";
                // Optionally log the error for debugging purposes
                // Logger.Log(ex);
            }

            return RedirectToAction("Index");
        }

        // Search Property Managers and Tenants
        public ActionResult Search(string searchTerm)
        {
            var results = db.Users.Where(u =>
                (u.Role.RoleName == "Tenant") &&
                (u.FirstName.Contains(searchTerm) || u.LastName.Contains(searchTerm) || u.Email.Contains(searchTerm))
            ).ToList();

            return View(results);
        }
    }
}
