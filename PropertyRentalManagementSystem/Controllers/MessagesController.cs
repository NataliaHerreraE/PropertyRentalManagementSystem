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
    [Authorize(Roles = "Property Manager")]
    public class MessagesController : Controller
    {
        private PropertyRentalManagementDBEntities db = new PropertyRentalManagementDBEntities();

        // GET: Messages
        public ActionResult Index()
        {
            var messages = db.Messages.Include(m => m.Apartment).Include(m => m.User).Include(m => m.User1).Include(m => m.TypeMessage);
            return View(messages.ToList());
        }

        // GET: Messages/Details/5
        public ActionResult Details(int? id)
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

        // GET: Messages/Edit/5
        public ActionResult Edit(int? id)
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
            ViewBag.ApartmentId = new SelectList(db.Apartments, "ApartmentId", "AppartmentNumber", message.ApartmentId);
            ViewBag.FromUserId = new SelectList(db.Users, "UserId", "FirstName", message.FromUserId);
            ViewBag.ToUserId = new SelectList(db.Users, "UserId", "FirstName", message.ToUserId);
            ViewBag.TypeId = new SelectList(db.TypeMessages, "TypeId", "TypeName", message.TypeId);
            return View(message);
        }

        // POST: Messages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MessageId,FromUserId,ToUserId,ApartmentId,Message1,DateSent,TypeId")] Message message)
        {
            if (ModelState.IsValid)
            {
                db.Entry(message).State = EntityState.Modified;
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
