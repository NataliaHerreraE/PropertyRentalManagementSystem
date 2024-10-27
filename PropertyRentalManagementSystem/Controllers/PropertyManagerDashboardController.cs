using PropertyRentalManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PropertyRentalManagementSystem.Controllers
{
    public class PropertyManagerDashboardController : Controller
    {
        private PropertyRentalManagementDBEntities db = new PropertyRentalManagementDBEntities();

        // GET: PropertyManager/Index
        public ActionResult Index()
        {
            // Check if UserId is present in the session
            if (Session["UserId"] == null)
            {
                // Redirect to the login page if the session has expired or user is not logged in
                return RedirectToAction("Login", "Account");
            }

            // Get the current user ID from the session
            int userId = (int)Session["UserId"];

            // Retrieve the user data from the database
            var user = db.Users.FirstOrDefault(u => u.UserId == userId);
            if (user != null)
            {
                // Assuming you have FirstName and LastName fields in the User model
                string userFullName = user.FirstName + " " + user.LastName;
                ViewBag.UserName = userFullName;
            }
            else
            {
                ViewBag.UserName = "User"; // Fallback in case user data is not found
            }

            // Count the total buildings managed by the property manager
            var buildingCount = db.Buildings.Count(b => b.PropertyManagerId == userId);

            // Count the total apartments managed by the property manager
            var apartmentCount = db.Apartments.Count(a => a.Building.PropertyManagerId == userId);

            // Count upcoming appointments
            var upcomingAppointmentsCount = db.Appointments
                .Count(a => a.Apartment.Building.PropertyManagerId == userId && a.Date > DateTime.Now);

            // Count unread messages for the current user
            var unreadMessagesCount = db.Messages
                .Count(m => m.ToUserId == userId && !m.IsRead);

            // Pass these counts to the view
            ViewBag.BuildingCount = buildingCount;
            ViewBag.ApartmentCount = apartmentCount;
            ViewBag.UpcomingAppointmentsCount = upcomingAppointmentsCount;
            ViewBag.UnreadMessagesCount = unreadMessagesCount;

            return View();

        }




        // Redirect to Buildings management
        public ActionResult ManageBuildings()
        {
            return RedirectToAction("Index", "Buildings");
        }

        // Redirect to Apartments management
        public ActionResult ManageApartments()
        {
            return RedirectToAction("Index", "Apartments");
        }

        // Redirect to Appointments management
        public ActionResult ManageAppointments()
        {
            return RedirectToAction("Index", "Appointments");
        }

        // Redirect to Messages management
        public ActionResult ManageMessages()
        {
            return RedirectToAction("Index", "Messages");
        }
    }
}