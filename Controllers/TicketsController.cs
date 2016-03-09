using Microsoft.AspNet.Identity;
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
    [Authorize]
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Tickets
        public ActionResult Index()
        {
            return View(db.Tickets.ToList());
        }

        // GET: Tickets/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        // GET: Tickets/Create
        public ActionResult Create()
        {
            var tkt = new Ticket();
            tkt.AuthorId = User.Identity.GetUserId();
            tkt.Date = System.DateTimeOffset.Now;
            tkt.Deadline = tkt.Date.AddMonths(1);
            //tkt.Description = "";

            ViewBag.ProjectId = new SelectList(db.Projects.OrderBy(n=>n.Name), "Id", "Name");

            var defaultIssue = (int)db.IssueTypes.
                Where(z => z.Name == "Not Categorized").
                Select(p => p.Id).FirstOrDefault();     // note: Where() returns a list, so need FirstOrDefault() to get 1 object
            ViewBag.IssueTypeId = new SelectList(db.IssueTypes, "Id", "Name", defaultIssue);

            var defaultLeader = db.Users.Where(z => z.LastName == "Unassigned").Select(p => p.Id).FirstOrDefault();
            var users = db.Users.Where(z => z.IsGuest == false).Select(p => new { p.Id, p.DisplayName });
            ViewBag.LeaderId = new SelectList(users, "Id", "DisplayName",defaultLeader);

            var defaultPriority = (int)db.Priorities.Where(z => z.Name == "Unassessed").Select(p => p.Id).FirstOrDefault();
            ViewBag.PriorityId = new SelectList(db.Priorities, "Id", "Name", defaultPriority);


            var defaultStage = (int)db.Stages.Where(z => z.Name == "Unassigned").Select(p => p.Id).FirstOrDefault();
            ViewBag.StageId = new SelectList(db.Stages, "Id", "Name", defaultStage);

            return View(tkt);
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Date,Deadline,Description,RepositoryURL,Title,AuthorId,IssueTypeId,LeaderId,PriorityId,ProjectId,StageId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                db.Tickets.Add(ticket);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ProjectId = new SelectList(db.Projects.OrderBy(n => n.Name), "Id", "Name");

            var defaultIssue = (int)db.IssueTypes.
                Where(z => z.Name == "Unassigned").
                Select(p => p.Id).FirstOrDefault();     // note: Where() returns a list, so need FirstOrDefault() to get 1 object

            ViewBag.IssueTypeId = new SelectList(db.IssueTypes, "Id", "Name", defaultIssue);

            var users = db.Users.Where(z => z.IsGuest == false).Select(p => new { p.Id, p.DisplayName });
            ViewBag.LeaderId = new SelectList(users, "Id", "DisplayName");

            var defaultPriority = (int)db.Priorities.Where(z => z.Name == "Unassessed").Select(p => p.Id).FirstOrDefault();
            ViewBag.PriorityId = new SelectList(db.Priorities, "Id", "Name", defaultPriority);


            var defaultStage = (int)db.Stages.Where(z => z.Name == "Unassigned").Select(p => p.Id).FirstOrDefault();
            ViewBag.StageId = new SelectList(db.Stages, "Id", "Name", defaultStage);


            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }

            
            ViewBag.IssueTypeId = new SelectList(db.IssueTypes, "Id", "Name", ticket.IssueTypeId);

            var users = db.Users.Where(z => z.IsGuest == false).Select(p => new { p.Id, p.DisplayName });
            ViewBag.LeaderId = new SelectList(users, "Id", "DisplayName", ticket.LeaderId);

            ViewBag.PriorityId = new SelectList(db.Priorities, "Id", "Name", ticket.PriorityId);
            
            ViewBag.StageId = new SelectList(db.Stages, "Id", "Name", ticket.StageId);
            
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Date,Deadline,Description,RepositoryURL,Title,AuthorId,IssueTypeId,LeaderId,PriorityId,ProjectId,StageId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ticket).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IssueTypeId = new SelectList(db.IssueTypes, "Id", "Name", ticket.IssueTypeId);

            var users = db.Users.Where(z => z.IsGuest == false).Select(p => new { p.Id, p.DisplayName });
            ViewBag.LeaderId = new SelectList(users, "Id", "DisplayName", ticket.LeaderId);

            ViewBag.PriorityId = new SelectList(db.Priorities, "Id", "Name", ticket.PriorityId);

            ViewBag.StageId = new SelectList(db.Stages, "Id", "Name", ticket.StageId);

            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ticket ticket = db.Tickets.Find(id);
            db.Tickets.Remove(ticket);
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
