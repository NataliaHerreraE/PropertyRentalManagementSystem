using PropertyRentalManagementSystem.Models;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace PropertyRentalManagementSystem.Controllers
{
    public class ManagerController : Controller
    {
        private PropertyRentalManagementDBEntities db = new PropertyRentalManagementDBEntities();

        // GET: Manager/Index
        public ActionResult Index(string searchTerm)
        {

            if (Session["UserId"] == null)
            {

                return RedirectToAction("Login", "Account");
            }
            int userId = (int)Session["UserId"];

            var managers = db.Users.Where(u => u.Role.RoleName == "Property Manager");

            if (!string.IsNullOrEmpty(searchTerm))
            {
                managers = managers.Where(u => u.FirstName.Contains(searchTerm) || u.LastName.Contains(searchTerm));
            }

            return View(managers.ToList());
        }

        public ActionResult ListPropertyManagers()
        {
            var propertyManagers = db.Users.Where(u => u.Role.RoleName == "Property Manager").ToList();
            return View(propertyManagers);
        }

        // GET: Manager/Create
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Manager/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(User model)
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
                return RedirectToAction("Index");
            }
            ViewBag.Message = "Error: Unable to create Property Manager.";
            return View(model);
        }

        // Additional CRUD methods similar to TenantController can go here...

        // Update Property Manager - GET
        public ActionResult Edit(int id)
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
        public ActionResult Edit(User model)
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
                return RedirectToAction("Index");  // Redirect to the list after saving
            }

            ViewBag.Message = "Error: Unable to update Property Manager. Please correct the errors and try again.";
            // If the model state is invalid, re-render the view with the current model
            return View(model);
        }

        public ActionResult Delete(int id)
        {
            var manager = db.Users.Find(id);
            if (manager == null || manager.Role.RoleName != "Property Manager")
            {
                TempData["ErrorMessage"] = "Error: Property Manager not found.";
                return RedirectToAction("Index");
            }

            // Verificar dependencias antes de eliminar: revisa si el Property Manager tiene algún edificio asignado
            var buildingsManaged = db.Buildings.Any(b => b.PropertyManagerId == id);
            if (buildingsManaged)
            {
                TempData["ErrorMessage"] = "You cannot delete this Property Manager because they are responsible for one or more buildings.";
                return RedirectToAction("Index");
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

            return RedirectToAction("Index");
        }



    }
}
