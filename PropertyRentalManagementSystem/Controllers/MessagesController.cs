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
        public ActionResult Index(string fromUserSearch, string toUserSearch)
        {
            int userId = (int)Session["UserId"];

            // Load all messages for this user
            var messagesQuery = db.Messages
                .Include(m => m.Apartment)
                .Include(m => m.User) // FromUser
                .Include(m => m.User1) // ToUser
                .Include(m => m.TypeMessage);

            // Apply search filters if provided
            if (!string.IsNullOrEmpty(fromUserSearch))
            {
                messagesQuery = messagesQuery.Where(m => m.User.FirstName.Contains(fromUserSearch) || m.User.LastName.Contains(fromUserSearch));
            }
            if (!string.IsNullOrEmpty(toUserSearch))
            {
                messagesQuery = messagesQuery.Where(m => m.User1.FirstName.Contains(toUserSearch) || m.User1.LastName.Contains(toUserSearch));
            }

            // Separate messages into read and unread
            var unreadMessages = messagesQuery.Where(m => m.ToUserId == userId && !m.IsRead).ToList();
            var readMessages = messagesQuery.Where(m => m.ToUserId == userId && m.IsRead).ToList();

            // All messages with additional details
            var allMessages = messagesQuery.ToList();

            // ViewBag counts for display
            ViewBag.UnreadMessagesCount = unreadMessages.Count;
            ViewBag.ReadMessagesCount = readMessages.Count;

            var viewModel = new MessagesViewModel
            {
                UnreadMessages = unreadMessages,
                ReadMessages = readMessages,
                AllMessages = allMessages
            };

            return View(viewModel);
        }

        // View Message Action - marks message as read
        public ActionResult ViewMessage(int id)
        {
            var message = db.Messages.Find(id);
            if (message != null && !message.IsRead)
            {
                message.IsRead = true;
                db.SaveChanges();
            }
            return View(message);
        }


        // GET: Messages/Create
        public ActionResult Create()
        {
            ViewBag.ApartmentId = new SelectList(db.Apartments, "ApartmentId", "AppartmentNumber");
            ViewBag.FromUserId = new SelectList(db.Users, "UserId", "FirstName");
            ViewBag.ToUserId = new SelectList(db.Users, "UserId", "FirstName");
            ViewBag.TypeId = new SelectList(db.TypeMessages, "TypeId", "TypeName");
            return View();
        }

        // POST: Messages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MessageId,FromUserId,ToUserId,ApartmentId,Message1,DateSent,TypeId")] Message message)
        {
            if (ModelState.IsValid)
            {
                db.Messages.Add(message);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ApartmentId = new SelectList(db.Apartments, "ApartmentId", "AppartmentNumber", message.ApartmentId);
            ViewBag.FromUserId = new SelectList(db.Users, "UserId", "FirstName", message.FromUserId);
            ViewBag.ToUserId = new SelectList(db.Users, "UserId", "FirstName", message.ToUserId);
            ViewBag.TypeId = new SelectList(db.TypeMessages, "TypeId", "TypeName", message.TypeId);
            return View(message);
        }


        // GET: Messages/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Message message = db.Messages.Find(id);
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
            db.Messages.Remove(message);
            db.SaveChanges();
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
