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
            
            if (Session["UserId"] == null)
            {
                
                return RedirectToAction("Login", "Account");
            }
            int userId = (int)Session["UserId"];

     
            var user = db.Users.FirstOrDefault(u => u.UserId == userId);
            if (user != null)
            {
                
                string userFullName = user.FirstName + " " + user.LastName;
                ViewBag.UserName = userFullName;
            }
            else
            {
                ViewBag.UserName = "User";
            }

            var buildingCount = db.Buildings.Count(b => b.PropertyManagerId == userId);

            var apartmentCount = db.Apartments.Count(a => a.Building.PropertyManagerId == userId);

            var upcomingAppointmentsCount = db.Appointments
                .Count(a => a.Apartment.Building.PropertyManagerId == userId && a.Date > DateTime.Now);

     
            var unreadMessagesCount = db.Messages
                .Count(m => m.ToUserId == userId && !m.IsRead);

            
            ViewBag.BuildingCount = buildingCount;
            ViewBag.ApartmentCount = apartmentCount;
            ViewBag.UpcomingAppointmentsCount = upcomingAppointmentsCount;
            ViewBag.UnreadMessagesCount = unreadMessagesCount;

            return View();

        }



        public ActionResult ManageBuildings()
        {
            return RedirectToAction("Index", "Buildings");
        }

        public ActionResult ManageApartments()
        {
            return RedirectToAction("Index", "Apartments");
        }


        public ActionResult ManageAppointments()
        {
            return RedirectToAction("Index", "Appointments");
        }

        public ActionResult ManageMessages()
        {
            return RedirectToAction("Index", "Messages");
        }
    }
}