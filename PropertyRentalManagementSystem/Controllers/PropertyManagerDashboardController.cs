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

            var upcomingPendingAppointmentsCount = db.Appointments
                .Count(a => a.Apartment.Building.PropertyManagerId == userId && a.Date > DateTime.Now && a.Status.StatusName == "Pending");

            ViewBag.UpcomingAppointmentsCount = upcomingPendingAppointmentsCount;


            // Fetch unread messages count
            int unreadMessagesCount = db.Messages
                .Count(m => (m.ToUserId == userId || m.FromUserId == userId) && m.IsRead == false);

            // Set the counts to ViewBag for display on the dashboard
            ViewBag.BuildingCount = buildingCount;
            ViewBag.ApartmentCount = apartmentCount;
            ViewBag.UpcomingAppointmentsCount = upcomingPendingAppointmentsCount;
            ViewBag.UnreadMessagesCount = unreadMessagesCount;

            // Total Payments related to the Property Manager's Rental Agreements
            ViewBag.TotalPayments = db.Payments
                .Count(p => p.RentalAgreement.Apartment.Building.PropertyManagerId == userId);

            // Total Rental Agreements related to the Property Manager's Buildings
            ViewBag.RentalAgreementsCount = db.RentalAgreements
                                               .Where(r => r.Apartment.Building.PropertyManagerId == userId)
                                               .Count();



            return View();

        }

        public JsonResult GetUnreadMessagesCount()
        {
            if (Session["UserId"] == null)
            {
                return Json(new { count = 0 }, JsonRequestBehavior.AllowGet);
            }

            int userId = (int)Session["UserId"];

            // Count unread messages where the user is either the sender or the recipient
            int unreadMessagesCount = db.Messages
                .Where(m => (m.ToUserId == userId || m.FromUserId == userId) && m.IsRead == false)
                .Count();

            return Json(new { count = unreadMessagesCount }, JsonRequestBehavior.AllowGet);
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