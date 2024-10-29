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
    public class PaymentsController : Controller
    {
        private PropertyRentalManagementDBEntities db = new PropertyRentalManagementDBEntities();

        // GET: Payments
        public ActionResult Index(string tenantSearch, int? statusId, int? agreementId)
        {
            int propertyManagerId = (int)Session["UserId"];

            // Retrieve payments related to agreements managed by this property manager
            var payments = db.Payments
                .Include(p => p.Status)
                .Include(p => p.RentalAgreement)
                .Include(p => p.RentalAgreement.Apartment)
                .Include(p => p.RentalAgreement.Apartment.Building)
                .Include(p => p.User)
                .Where(p => p.RentalAgreement.Apartment.Building.PropertyManagerId == propertyManagerId);

            // Apply search filters
            if (!string.IsNullOrEmpty(tenantSearch))
            {
                payments = payments.Where(p => (p.User.FirstName + " " + p.User.LastName).Contains(tenantSearch));
            }

            if (statusId.HasValue)
            {
                payments = payments.Where(p => p.StatusId == statusId.Value);
            }

            if (agreementId.HasValue)
            {
                payments = payments.Where(p => p.AgreementId == agreementId.Value);
            }

            // Count payments by status for summary cards
            ViewBag.ApprovedPaymentsCount = payments.Count(p => p.Status.StatusName == "Approved");
            ViewBag.CancelledPaymentsCount = payments.Count(p => p.Status.StatusName == "Cancelled");
            ViewBag.InProgressPaymentsCount = payments.Count(p => p.Status.StatusName == "In Progress");
            ViewBag.RejectedPaymentsCount = payments.Count(p => p.Status.StatusName == "Rejected");

            // Populate dropdown lists for filtering
            ViewBag.StatusId = new SelectList(db.Status.Where(s => s.StatusName == "Approved" || s.StatusName == "Cancelled" || s.StatusName == "In Progress" || s.StatusName == "Rejected"), "StatusId", "StatusName", statusId);
            ViewBag.AgreementId = new SelectList(db.RentalAgreements
                .Where(ra => ra.Apartment.Building.PropertyManagerId == propertyManagerId)
                .Select(ra => new
                {
                    ra.AgreementId,
                    DisplayText = "Agreement #" + ra.AgreementId + " - " + ra.Apartment.Building.BuildingName + " - Apt " + ra.Apartment.AppartmentNumber
                }),
                "AgreementId", "DisplayText", agreementId);

            return View(payments.ToList());
        }



        // GET: Payments/Details/5
        public ActionResult Details(int id)
        {
            var payment = db.Payments
                .Include(p => p.User) // Tenant info
                .Include(p => p.Status)
                .Include(p => p.RentalAgreement)
                .Include(p => p.RentalAgreement.Apartment)
                .Include(p => p.RentalAgreement.Apartment.Building)
                .FirstOrDefault(p => p.PaymentId == id);

            if (payment == null)
            {
                return HttpNotFound();
            }

            return View(payment);
        }

        // GET: Payments/Create
        public ActionResult Create()
        {
            ViewBag.AgreementId = new SelectList(db.RentalAgreements, "AgreementId", "AgreementId");
            ViewBag.StatusId = new SelectList(db.Status, "StatusId", "StatusName");
            ViewBag.UserId = new SelectList(db.Users, "UserId", "FirstName");
            return View();
        }

        // POST: Payments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PaymentId,UserId,AgreementId,Amount,DatePaid,MethodOfPayment,StatusId")] Payment payment)
        {
            if (ModelState.IsValid)
            {
                db.Payments.Add(payment);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AgreementId = new SelectList(db.RentalAgreements, "AgreementId", "AgreementId", payment.AgreementId);
            ViewBag.StatusId = new SelectList(db.Status, "StatusId", "StatusName", payment.StatusId);
            ViewBag.UserId = new SelectList(db.Users, "UserId", "FirstName", payment.UserId);
            return View(payment);
        }

        // GET: Payments/Edit/5
        public ActionResult Edit(int id)
        {
            var payment = db.Payments
                            .Include(p => p.User)
                            .Include(p => p.RentalAgreement.Apartment.Building)
                            .FirstOrDefault(p => p.PaymentId == id);

            if (payment == null)
            {
                TempData["ErrorMessage"] = "Payment not found.";
                return RedirectToAction("Index");
            }

            // Filter statuses to include only the specified options
            ViewBag.StatusId = new SelectList(db.Status.Where(s =>
                s.StatusName == "Approved" ||
                s.StatusName == "Cancelled" ||
                s.StatusName == "In Progress" ||
                s.StatusName == "Rejected"), "StatusId", "StatusName", payment.StatusId);

            // Populate Method of Payment dropdown
            ViewBag.MethodOfPayments = new List<string> { "Credit Card", "PayPal", "Bank Transfer", "Cash" };

            return View(payment);
        }

        // POST: Payments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PaymentId,Amount,StatusId,MethodOfPayment")] Payment payment)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var paymentInDb = db.Payments.Find(payment.PaymentId);
                    if (paymentInDb != null)
                    {
                        paymentInDb.Amount = payment.Amount;
                        paymentInDb.StatusId = payment.StatusId;
                        paymentInDb.MethodOfPayment = payment.MethodOfPayment;
                        db.SaveChanges();

                        TempData["SuccessMessage"] = "Payment updated successfully!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Payment not found.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An error occurred while saving the payment: {ex.Message}");
                }
            }

            // Repopulate Status and Method of Payment dropdowns in case of validation errors
            ViewBag.StatusId = new SelectList(db.Status.Where(s =>
                s.StatusName == "Approved" ||
                s.StatusName == "Cancelled" ||
                s.StatusName == "In Progress" ||
                s.StatusName == "Rejected"), "StatusId", "StatusName", payment.StatusId);

            ViewBag.MethodOfPayments = new List<string> { "Debit Card", "Credit Card", "PayPal", "Bank Transfer", "Cash" };

            return View(payment);
        }




        // GET: Payments/Delete/5
        public ActionResult Delete(int id)
        {
            var payment = db.Payments
                            .Include(p => p.User)
                            .Include(p => p.Status)
                            .Include(p => p.RentalAgreement.Apartment.Building)
                            .FirstOrDefault(p => p.PaymentId == id);

            if (payment == null)
            {
                TempData["ErrorMessage"] = "Payment not found.";
                return RedirectToAction("Index");
            }

            return View(payment); // Renders the confirmation page
        }

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                var payment = db.Payments.Find(id);
                if (payment != null)
                {
                    db.Payments.Remove(payment);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Payment deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Payment not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the payment: " + ex.Message;
            }

            return RedirectToAction("Index"); // Redirects to Index with success or error message
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
