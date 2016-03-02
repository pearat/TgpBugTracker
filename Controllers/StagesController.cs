using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TgpBugTracker.Models;

namespace TgpBugTracker.Controllers
{
    public class StagesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Stages
        public ActionResult Index()
        {
            return View(db.Stages.ToList());
        }

        // GET: Stages/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Stage stage = db.Stages.Find(id);
            if (stage == null)
            {
                return HttpNotFound();
            }
            return View(stage);
        }

        // GET: Stages/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Stages/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name")] Stage stage)
        {
            if (ModelState.IsValid)
            {
                db.Stages.Add(stage);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(stage);
        }

        // GET: Stages/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Stage stage = db.Stages.Find(id);
            if (stage == null)
            {
                return HttpNotFound();
            }
            return View(stage);
        }

        // POST: Stages/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name")] Stage stage)
        {
            if (ModelState.IsValid)
            {
                db.Entry(stage).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(stage);
        }

        // GET: Stages/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Stage stage = db.Stages.Find(id);
            if (stage == null)
            {
                return HttpNotFound();
            }
            return View(stage);
        }

        // POST: Stages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Stage stage = db.Stages.Find(id);
            db.Stages.Remove(stage);
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
