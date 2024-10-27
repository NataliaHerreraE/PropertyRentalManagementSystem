using PropertyRentalManagementSystem.Models;
using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace PropertyRentalManagementSystem.Controllers
{
    public class OwnersAdministratorsDashboardController : Controller
    {
        private PropertyRentalManagementDBEntities db = new PropertyRentalManagementDBEntities();

        // GET: OwnersAdministratorsDashboard/Index
        public ActionResult Index()
        {

            if (Session["UserId"] == null)
            {

                return RedirectToAction("Login", "Account");
            }
            int userId = (int)Session["UserId"];

            if (Session["RoleName"] != null &&
                (Session["RoleName"].ToString() == "Property Owner" ||
                 Session["RoleName"].ToString() == "Administrator"))
            {
                // Pass data to the dashboard
                ViewBag.TenantCount = db.Users.Count(u => u.Role.RoleName == "Tenant");
                ViewBag.ManagerCount = db.Users.Count(u => u.Role.RoleName == "Property Manager");
                ViewBag.BuildingCount = db.Buildings.Count();

                return View();
            }
            else
            {
                // If unauthorized, redirect to login
                return RedirectToAction("Login", "Account");
            }
        }
        // Property Manager Actions
/*
        // GET: OwnersAdministratorsDashboard/ManagerView
        public ActionResult ManagerView()
        {
            var managers = db.Users.Where(u => u.Role.RoleName == "Property Manager").ToList();
            return View(managers);
        }*/
        // GET: OwnersAdministratorsDashboard/ManagerView
/*        public ActionResult ManagerView(string searchTerm)
        {
            var managers = db.Users.Where(u => u.Role.RoleName == "Property Manager");

            if (!string.IsNullOrEmpty(searchTerm))
            {
                managers = managers.Where(u => u.FirstName.Contains(searchTerm) || u.LastName.Contains(searchTerm));
            }

            return View(managers.ToList());
        }
*/
        // GET: OwnersAdministratorsDashboard/TenantView
/*        public ActionResult TenantView(string searchTerm)
        {
            var tenants = db.Users.Where(u => u.Role.RoleName == "Tenant");

            if (!string.IsNullOrEmpty(searchTerm))
            {
                tenants = tenants.Where(u => u.FirstName.Contains(searchTerm) || u.LastName.Contains(searchTerm));
            }

            return View(tenants.ToList());
        }

        // List Property Managers
        public ActionResult ListPropertyManagers()
        {
            var propertyManagers = db.Users.Where(u => u.Role.RoleName == "Property Manager").ToList();
            return View(propertyManagers);
        }

        // Create Property Manager - GET
        [HttpGet]
        public ActionResult CreatePropertyManager()
        {
            return View();
        }

        // Create Property Manager - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePropertyManager(User model)
        {
            if (ModelState.IsValid)
            {
                // Check if a user with the same email already exists
                var existingUser = db.Users.FirstOrDefault(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "A user with this email already exists.");
                    ViewBag.Message = "Failed to create Property Manager: Email already exists.";
                    return View(model);  // Re-render the form with error
                }

                // Ensure the role is assigned correctly
                model.RoleId = db.Roles.FirstOrDefault(r => r.RoleName == "Property Manager").RoleId;

                // Add and save the new user
                try
                {
                    db.Users.Add(model);  // Only add, no need to modify
                    db.SaveChanges();      // Save changes to persist the new user

                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException?.InnerException is SqlException sqlEx && sqlEx.Number == 2627) // Unique constraint violation
                    {
                        ModelState.AddModelError("", "A user with the same key already exists.");
                        return View(model);
                    }

                    throw;  // Re-throw if it's a different exception
                }
                TempData["Message"] = "Property Manager created successfully!";
                return RedirectToAction("ManagerView");
            }
            ViewBag.Message = "Error: Unable to create Property Manager.";
            return View(model);
        }


        // Update Property Manager - GET
        public ActionResult EditPropertyManager(int id)
        {
            // Find the manager by ID and include the Role
            var manager = db.Users.Include(u => u.Role).FirstOrDefault(u => u.UserId == id);

            if (manager == null)
            {
                return HttpNotFound();  // User not found
            }

            // Check if the user is a property manager
            if (manager.Role.RoleName != "Property Manager")
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "You are not allowed to edit this user.");
            }

            return View(manager);
        }



        // Update Property Manager - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPropertyManager(User model)
        {
            // Validate if all fields are provided (not null or empty)
            if (string.IsNullOrWhiteSpace(model.FirstName) ||
                string.IsNullOrWhiteSpace(model.LastName) ||
                string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.Phone))
            {
                ViewBag.Message = "Error: All fields must be filled in.";
                ModelState.AddModelError("", "All fields are required.");
                return View(model);  // Return the form with error message
            }

            if (ModelState.IsValid)
            {
                // Find the existing Property Manager by ID
                var manager = db.Users.Find(model.UserId);

                if (manager == null)
                {
                    ViewBag.Message = "Error: Property Manager not found.";
                    return HttpNotFound();
                }

                // Check if the email has changed
                if (manager.Email != model.Email)
                {
                    // Check if the new email is already used by another user
                    var existingUserWithSameEmail = db.Users.FirstOrDefault(u => u.Email == model.Email && u.UserId != model.UserId);
                    if (existingUserWithSameEmail != null)
                    {
                        // Add error to ModelState and return the view
                        ModelState.AddModelError("Email", "This email address is already taken by another user.");
                        ViewBag.Message = "Error: Email is already in use.";
                        return View(model);  // Re-render the form with the error
                    }

                    // If the email is unique, update the email
                    manager.Email = model.Email;
                }

                // Update the other fields of the Property Manager
                manager.FirstName = model.FirstName;
                manager.LastName = model.LastName;
                manager.Phone = model.Phone;

                // Mark the entity as modified
                db.Entry(manager).State = EntityState.Modified;

                // Save the changes to the database
                db.SaveChanges();

                TempData["Message"] = "Property Manager updated successfully!";
                return RedirectToAction("ManagerView");  // Redirect to the list after saving
            }

            ViewBag.Message = "Error: Unable to update Property Manager. Please correct the errors and try again.";
            // If the model state is invalid, re-render the view with the current model
            return View(model);
        }

*/




        /*      // Delete Property Manager
              public ActionResult DeletePropertyManager(int id)
              {
                  var manager = db.Users.Find(id);
                  if (manager == null || manager.Role.RoleName != "Property Manager")
                  {
                      return HttpNotFound();
                  }

                  // Verificar dependencias antes de eliminar: revisa si el Property Manager tiene algún edificio asignado
                  var buildingsManaged = db.Buildings.Any(b => b.PropertyManagerId == id);
                  if (buildingsManaged)
                  {
                      ModelState.AddModelError("", "You cannot delete this Property Manager because they are responsible for one or more buildings.");
                      return RedirectToAction("ManagerView");
                  }

                  // Si no tiene dependencias, procede a eliminar
                  db.Users.Remove(manager);
                  db.SaveChanges();

                  return RedirectToAction("ManagerView");
              }

      */
        // Delete Property Manager
/*        public ActionResult DeletePropertyManager(int id)
        {
            var manager = db.Users.Find(id);
            if (manager == null || manager.Role.RoleName != "Property Manager")
            {
                TempData["ErrorMessage"] = "Error: Property Manager not found.";
                return RedirectToAction("ManagerView");
            }

            // Verificar dependencias antes de eliminar: revisa si el Property Manager tiene algún edificio asignado
            var buildingsManaged = db.Buildings.Any(b => b.PropertyManagerId == id);
            if (buildingsManaged)
            {
                TempData["ErrorMessage"] = "You cannot delete this Property Manager because they are responsible for one or more buildings.";
                return RedirectToAction("ManagerView");
            }

            try
            {
                // Si no tiene dependencias, procede a eliminar
                db.Users.Remove(manager);
                db.SaveChanges();

                TempData["Message"] = "Property Manager deleted successfully!";
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during SaveChanges
                TempData["ErrorMessage"] = "Error: Unable to delete the Property Manager. Please try again.";
                // Optionally, log the error for debugging purposes
                // Logger.Log(ex);
            }

            return RedirectToAction("ManagerView");
        }
*/

/*

        // Potential Tenants Actions

        // List Potential Tenants
        public ActionResult ListTenants()
        {
            var tenant = db.Users.Where(u => u.Role.RoleName == "Tenant").ToList();
            return View(tenant);
        }


        // Edit Tenant - GET
        public ActionResult EditTenant(int id)
        {
            var tenant = db.Users.Find(id);
            if (tenant == null || tenant.Role.RoleName != "Tenant")
            {
                return HttpNotFound();
            }
            return View(tenant);
        }



        // Edit Tenant - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTenant(User model)
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
                    return RedirectToAction("TenantView");
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






        // Delete Tenant
        public ActionResult DeleteTenant(int id)
        {
            var tenant = db.Users.Find(id);
            if (tenant == null || tenant.Role.RoleName != "Tenant")
            {
                TempData["ErrorMessage"] = "Error: Tenant not found.";
                return RedirectToAction("TenantView");
            }

            // Check if the tenant is linked to an apartment or any other related record
            var tenantHasApartment = db.RentalAgreements.Any(ra => ra.TenantId == id);
            if (tenantHasApartment)
            {
                TempData["ErrorMessage"] = "You cannot delete this Tenant because they are linked to one or more apartments.";
                return RedirectToAction("TenantView");
            }

            try
            {
                // Proceed with the deletion if no dependencies are found
                db.Users.Remove(tenant);
                db.SaveChanges();
                TempData["Message"] = "Tenant deleted successfully!";
            }
            catch (Exception )
            {
                TempData["ErrorMessage"] = "Error: Unable to delete the tenant. It may be linked to other records in the system.";
                // Optionally log the error for debugging purposes
                // Logger.Log(ex);
            }

            return RedirectToAction("TenantView");
        }


        // Search Property Managers and Tenants
        public ActionResult SearchUsers(string searchTerm)
        {
            var results = db.Users.Where(u =>
                (u.Role.RoleName == "Property Manager" || u.Role.RoleName == "Tenant") &&
                (u.FirstName.Contains(searchTerm) || u.LastName.Contains(searchTerm) || u.Email.Contains(searchTerm))
            ).ToList();

            return View(results);
        }*/
    }



}
