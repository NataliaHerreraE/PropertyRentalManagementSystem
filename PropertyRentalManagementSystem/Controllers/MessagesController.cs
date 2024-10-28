using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PropertyRentalManagementSystem.Models;
using PropertyRentalManagementSystem.ViewModels;

namespace PropertyRentalManagementSystem.Controllers
{
    //[Authorize(Roles = "Property Manager")]
    public class MessagesController : Controller
    {
        private PropertyRentalManagementDBEntities db = new PropertyRentalManagementDBEntities();

        // GET: Messages
        public ActionResult Index(string fromUserSearch, string toUserSearch, int? buildingId, int? apartmentId)
        {
            int userId = (int)Session["UserId"];

            var messagesQuery = db.Messages
                .Include(m => m.Apartment)
                .Include(m => m.Apartment.Building)
                .Include(m => m.User)
                .Include(m => m.User1)
                .Include(m => m.TypeMessage)
                .Where(m => m.FromUserId == userId || m.ToUserId == userId);

            // Apply filters if provided
            if (!string.IsNullOrEmpty(fromUserSearch))
            {
                messagesQuery = messagesQuery.Where(m => m.User.FirstName.Contains(fromUserSearch) || m.User.LastName.Contains(fromUserSearch));
            }
            if (!string.IsNullOrEmpty(toUserSearch))
            {
                messagesQuery = messagesQuery.Where(m => m.User1.FirstName.Contains(toUserSearch) || m.User1.LastName.Contains(toUserSearch));
            }
            if (buildingId.HasValue)
            {
                messagesQuery = messagesQuery.Where(m => m.Apartment.BuildingId == buildingId.Value);
            }
            if (apartmentId.HasValue)
            {
                messagesQuery = messagesQuery.Where(m => m.ApartmentId == apartmentId.Value);
            }

            var unreadMessages = messagesQuery.Where(m => m.IsRead == false).ToList();
            var readMessages = messagesQuery.Where(m => m.IsRead == true).ToList();

            ViewBag.UnreadMessagesCount = unreadMessages.Count;
            ViewBag.ReadMessagesCount = readMessages.Count;

            // Populate the dropdown lists for buildings and apartments
            ViewBag.BuildingId = new SelectList(db.Buildings, "BuildingId", "BuildingName");
            ViewBag.ApartmentId = new SelectList(db.Apartments, "ApartmentId", "AppartmentNumber");

            var viewModel = new MessagesViewModel
            {
                UnreadMessages = unreadMessages,
                ReadMessages = readMessages,
                AllMessages = unreadMessages.Concat(readMessages).ToList()
            };

            return View(viewModel);
        }




        public ActionResult Details(int id)
        {
            var message = db.Messages
                .Include(m => m.User)
                .Include(m => m.User1)
                .Include(m => m.Apartment)
                .Include(m => m.Apartment.Building) // Ensure Building is included
                .Include(m => m.TypeMessage)
                .FirstOrDefault(m => m.MessageId == id);

            if (message == null)
            {
                return HttpNotFound();
            }

            // Mark message as read if not already
            if (!message.IsRead)
            {
                message.IsRead = true;
                db.SaveChanges();
            }

            return View("Details", message);
        }




        // GET: Messages/Create
        public ActionResult Create(int? toUserId)
        {
            int fromUserId = (int)Session["UserId"]; // Automatically set the logged-in user as the sender

            // Create a SelectList for apartments with combined ApartmentNumber and BuildingName
            var apartments = db.Apartments
                .Select(a => new
                {
                    ApartmentId = a.ApartmentId,
                    DisplayText = a.AppartmentNumber + " - " + a.Building.BuildingName
                }).ToList();

            ViewBag.ApartmentId = new SelectList(apartments, "ApartmentId", "DisplayText");
            ViewBag.ToUserId = new SelectList(db.Users, "UserId", "FirstName", toUserId);
            ViewBag.TypeId = new SelectList(db.TypeMessages, "TypeId", "TypeName");

            return View();
        }


        // POST: Messages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MessageId,ToUserId,ApartmentId,Message1,TypeId")] Message message)
        {
            if (ModelState.IsValid)
            {
                // Set the sender ID from the logged-in user (replace "UserId" with your actual session key if different)
                int fromUserId = (int)Session["UserId"];
                message.FromUserId = fromUserId;

                // Set the current date and time for DateSent
                message.DateSent = DateTime.Now;

                db.Messages.Add(message);
                try
                {
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    // Log or handle the exception as needed
                    ModelState.AddModelError("", "An error occurred while saving the message. Ensure all required fields are valid.");
                }
            }

            // If model is invalid, or an error occurred, re-populate dropdowns and return to the view
            var apartments = db.Apartments
                .Select(a => new
                {
                    ApartmentId = a.ApartmentId,
                    DisplayText = a.AppartmentNumber + " - " + a.Building.BuildingName
                }).ToList();
            ViewBag.ApartmentId = new SelectList(apartments, "ApartmentId", "DisplayText", message.ApartmentId);
            ViewBag.ToUserId = new SelectList(db.Users, "UserId", "FirstName", message.ToUserId);
            ViewBag.TypeId = new SelectList(db.TypeMessages, "TypeId", "TypeName", message.TypeId);

            return View(message);
        }



        public JsonResult GetApartmentsByBuilding(int buildingId)
        {
            var apartments = db.Apartments
                               .Where(a => a.BuildingId == buildingId)
                               .Select(a => new { a.ApartmentId, a.AppartmentNumber })
                               .ToList();
            return Json(apartments, JsonRequestBehavior.AllowGet);
        }

        // GET: Messages/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Message message = db.Messages
                                .Include(m => m.User)
                                .Include(m => m.User1)
                                .Include(m => m.Apartment)
                                .Include(m => m.TypeMessage)
                                .FirstOrDefault(m => m.MessageId == id);

            if (message == null)
            {
                return HttpNotFound();
            }
            return View(message);
        }

        // POST: Messages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Message message = db.Messages.Find(id);
            if (message != null)
            {
                db.Messages.Remove(message);
                db.SaveChanges();
            }
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
