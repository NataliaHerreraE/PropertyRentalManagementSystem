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
            // Count the total buildings managed by the property manager
            var buildingCount = db.Buildings.Count();

            // Count the total apartments managed by the property manager
            var apartmentCount = db.Apartments.Count();

            // Count upcoming appointments
            var upcomingAppointmentsCount = db.Appointments.Count(a => a.Date > DateTime.Now);

            // Count unread messages (assuming there's a field `IsRead` in the Message model)
            var unreadMessagesCount = db.Messages.Count(m => m.ToUserId == (int)Session["UserId"] && !m.IsRead);

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