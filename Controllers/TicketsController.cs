using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TgpBugTracker.Helpers;
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
            var user = db.Users.Find(User.Identity.GetUserId());
            if (user == null)
            {
                ViewBag.FullName = "Pls login";
                ViewBag.Greeting = "Hi, ???";
            }
            else
            {
                ViewBag.FullName = user.FullName;
                ViewBag.Greeting = user.Greeting;
            }

            var uHelper = new ProjectUsersHelper();
            var rHelper = new UserRolesHelper();
            var UserLevel = rHelper.GetUsersAuthorizationLevel(user.Id);

            ViewBag.SubmitterOnly = (UserLevel == (int)UserRolesHelper.AuthLevel.Submitter) ? true : false;
            ViewBag.UserId = user.Id;
            
            var TicketList = uHelper.ListTicketsForUser(ViewBag.UserId);
            if (TicketList == null)
            {
                TicketList = new List<Ticket>();
                return View();
            }
            else
                return View(TicketList);
        }

        // GET: Tickets
        //public ActionResult Index(int? projectId)
        //{
        //    int AllMyProjects = 999;
        //    var ProjectId = (projectId == null) ? AllMyProjects : projectId;

        //    if (ProjectId == AllMyProjects)
        //    {
        //        return View(db.Tickets.ToList().OrderBy(m => m.Project.Name).ThenBy(n => n.Date));
        //        }
        //    else
        //    {
        //        // add checking to see whether user is authorized to view this project
        //        return View(db.Tickets.Where(i => i.ProjectId == ProjectId).ToList().OrderBy(n => n.Date));
        //    }
        //}

        // GET: Tickets
        //public ActionResult _ChooseProject()
        //{
        //    var user = db.Users.Find(User.Identity.GetUserId());
        //    var helper = new ProjectUsersHelper();

        //    var ProjectList = helper.ListProjectsForUser(user.Id);

        //    ViewBag.PossibleProjects = new SelectList(ProjectList, "Id", "Name");

        //    return View();
        //        // RedirectToAction("Index", new { projectId = ProjectId });
        //}

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

            ViewBag.ProjectId = new SelectList(db.Projects.OrderBy(n => n.Name), "Id", "Name");

            var defaultIssue = (int)db.IssueTypes.
                Where(z => z.Name == "Unassessed").
                Select(p => p.Id).FirstOrDefault();     // note: Where() returns a list, so need FirstOrDefault() to get 1 object
            ViewBag.IssueTypeId = new SelectList(db.IssueTypes, "Id", "Name", defaultIssue);

            var defaultLeader = db.Users.Where(z => z.LastName == "Unassigned").Select(p => p.Id).FirstOrDefault();
            var users = db.Users.Where(z => z.IsGuest == false).Select(p => new { p.Id, p.DisplayName });
            ViewBag.LeaderId = new SelectList(users, "Id", "DisplayName", defaultLeader);

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
            
            ViewBag.IssueTypeId = new SelectList(db.IssueTypes.OrderBy(p => p.Name), "Id", "Name", ticket.IssueTypeId);

            var users = db.Users.Where(z => z.IsGuest == false).Select(p => new { p.Id, p.DisplayName }).OrderBy(j => j.DisplayName).ToList();
            ViewBag.LeaderId = new SelectList(users, "Id", "DisplayName", ticket.LeaderId);

            ViewBag.PriorityId = new SelectList(db.Priorities.OrderBy(p => p.Name), "Id", "Name", ticket.PriorityId);

            ViewBag.StageId = new SelectList(db.Stages.OrderBy(p => p.Name), "Id", "Name", ticket.StageId);

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

            ViewBag.IssueTypeId = new SelectList(db.IssueTypes.OrderBy(p => p.Name), "Id", "Name", ticket.IssueTypeId);

            var users = db.Users.Where(z => z.IsGuest == false).Select(p => new { p.Id, p.DisplayName }).OrderBy(j=>j.DisplayName).ToList();
            ViewBag.LeaderId = new SelectList(users, "Id", "DisplayName", ticket.LeaderId);

            ViewBag.PriorityId = new SelectList(db.Priorities.OrderBy(p=>p.Name), "Id", "Name", ticket.PriorityId);

            ViewBag.StageId = new SelectList(db.Stages.OrderBy(p => p.Name), "Id", "Name", ticket.StageId);

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
