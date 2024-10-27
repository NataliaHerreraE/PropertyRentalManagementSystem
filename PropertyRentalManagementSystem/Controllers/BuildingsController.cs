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
    public class BuildingsController : Controller
    {
        private PropertyRentalManagementDBEntities db = new PropertyRentalManagementDBEntities();

        // GET: Buildings
        public ActionResult Index(string searchTerm)
        {
            // Check if UserId is present in the session
            if (Session["UserId"] == null)
            {
                // Redirect to the login page if the session has expired or user is not logged in
                return RedirectToAction("Login", "Account");
            }

            // Get the current user ID
            int userId = (int)Session["UserId"];

            // Fetch buildings associated with the current property manager
            var buildings = db.Buildings
                .Where(b => b.PropertyManagerId == userId);

            // If there is a search term, filter by BuildingName or City
            if (!string.IsNullOrEmpty(searchTerm))
            {
                buildings = buildings
                    .Where(b => b.BuildingName.Contains(searchTerm) || b.City.Contains(searchTerm));
            }

            return View(buildings.ToList());
        }



        // GET: Buildings/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Building building = db.Buildings.Find(id);
            if (building == null)
            {
                return HttpNotFound();
            }
            return View(building);
        }

        // GET: Buildings/Create
        public ActionResult Create()
        {
            ViewBag.PropertyManagerId = new SelectList(db.Users, "UserId", "FirstName");
            return View();
        }

        // POST: Buildings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "BuildingId,BuildingName,Address,City,PostalCode,DateListed,PropertyManagerId,ImagePath")] Building building)
        {
            if (ModelState.IsValid)
            {
                db.Buildings.Add(building);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.PropertyManagerId = new SelectList(db.Users, "UserId", "FirstName", building.PropertyManagerId);
            return View(building);
        }

        // GET: Buildings/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Building building = db.Buildings.Find(id);
            if (building == null)
            {
                return HttpNotFound();
            }
            ViewBag.PropertyManagerId = new SelectList(db.Users, "UserId", "FirstName", building.PropertyManagerId);
            return View(building);
        }

        // POST: Buildings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "BuildingId,BuildingName,Address,City,PostalCode,DateListed,PropertyManagerId,ImagePath")] Building building)
        {
            if (ModelState.IsValid)
            {
                db.Entry(building).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.PropertyManagerId = new SelectList(db.Users, "UserId", "FirstName", building.PropertyManagerId);
            return View(building);
        }

        // GET: Buildings/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Building building = db.Buildings.Find(id);
            if (building == null)
            {
                return HttpNotFound();
            }
            return View(building);
        }

        // POST: Buildings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Building building = db.Buildings.Find(id);
            db.Buildings.Remove(building);
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
