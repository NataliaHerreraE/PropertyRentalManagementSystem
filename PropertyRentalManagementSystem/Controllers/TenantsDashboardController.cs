using PropertyRentalManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PropertyRentalManagementSystem.ViewModels;

namespace PropertyRentalManagementSystem.Controllers
{
    //[Authorize(Roles = "Tenant")]
    public class TenantsDashboardController : Controller
    {
        private PropertyRentalManagementDBEntities db = new PropertyRentalManagementDBEntities();

        // GET: TenantsDashboard
        public ActionResult Index()
        {
            int userId = (int)Session["UserId"];

            ViewBag.UserName = db.Users.Find(userId).FirstName;

            // Get counts for each entity relevant to the tenant
            ViewBag.AvailableApartmentCount = db.Apartments.Count(a => a.StatusId == 1);
            ViewBag.MessageCount = db.Messages.Count(m => m.FromUserId == userId || m.ToUserId == userId);
            ViewBag.AppointmentCount = db.Appointments.Count(a => a.TenantId == userId);
            ViewBag.RentalAgreementsCount = db.RentalAgreements.Count(r => r.TenantId == userId);
            ViewBag.PaymentCount = db.Payments.Count(p => p.UserId == userId);

            return View();
        }
    }
}