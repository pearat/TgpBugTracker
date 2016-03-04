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
    [RequireHttps]
    public class IssueTypesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: IssueTypes
        public ActionResult Index()
        {
            return View(db.IssueTypes.ToList());
        }

        // GET: IssueTypes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            IssueType issueType = db.IssueTypes.Find(id);
            if (issueType == null)
            {
                return HttpNotFound();
            }
            return View(issueType);
        }

        // GET: IssueTypes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: IssueTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name")] IssueType issueType)
        {
            if (ModelState.IsValid)
            {
                db.IssueTypes.Add(issueType);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(issueType);
        }

        // GET: IssueTypes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            IssueType issueType = db.IssueTypes.Find(id);
            if (issueType == null)
            {
                return HttpNotFound();
            }
            return View(issueType);
        }

        // POST: IssueTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name")] IssueType issueType)
        {
            if (ModelState.IsValid)
            {
                db.Entry(issueType).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(issueType);
        }

        // GET: IssueTypes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            IssueType issueType = db.IssueTypes.Find(id);
            if (issueType == null)
            {
                return HttpNotFound();
            }
            return View(issueType);
        }

        // POST: IssueTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            IssueType issueType = db.IssueTypes.Find(id);
            db.IssueTypes.Remove(issueType);
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
