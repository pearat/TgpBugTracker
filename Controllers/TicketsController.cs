using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
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
        public ActionResult Index(bool showArchived)
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
            var UserRank = rHelper.GetRoleRank(user.Id);

            ViewBag.SubmitterOnly = (UserRank == (int)UserRolesHelper.RoleRank.Submitter) ? true : false;
            ViewBag.UserId = user.Id;

            var TicketList = uHelper.ListTicketsForUser(ViewBag.UserId, showArchived);
            
            if (TicketList == null)
            {
                TicketList = new List<Ticket>();
                return View();
            }
            else
                return View(TicketList);
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
            ViewBag.UserId = User.Identity.GetUserId();
            return View(ticket);
        }

        // GET: Tickets/Create
        public ActionResult Create()
        {
            var tkt = new Ticket();
            tkt.AuthorId = User.Identity.GetUserId();
            tkt.Date = System.DateTimeOffset.Now;
            tkt.Deadline = tkt.Date.AddMonths(1);
            var user = db.Users.Find(User.Identity.GetUserId());
            var uHelper = new ProjectUsersHelper();
            ViewBag.ProjectId = new SelectList(uHelper.ListProjectsForUser(user.Id), "Id", "Name");

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
                return RedirectToAction("Index", new { showArchived = false });
            }
            /*
            { "The INSERT statement conflicted with the FOREIGN KEY constraint \"FK_dbo.Tickets_dbo.IssueTypes_IssueTypeId\". 
            The conflict occurred in database \"tpeara-bugtracker\", table \"dbo.IssueTypes\", column 'Id'.
            \r\nThe statement has been terminated."}
            */
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

            var project = db.Projects.Find(ticket.ProjectId);
            var devId = db.Roles.FirstOrDefault(r => r.Name == "Developer").Id;
            var possibleTeamMembers = project.Users.Where(u => u.IsGuest == false && u.Roles.Any(z=>z.RoleId == devId));

            ViewBag.LeaderId = new SelectList(possibleTeamMembers, "Id", "DisplayName", ticket.LeaderId);

            ViewBag.IssueTypeId = new SelectList(db.IssueTypes.OrderBy(p => p.Name), "Id", "Name", ticket.IssueTypeId);

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
                return RedirectToAction("Index", new { showArchived = false });
            }

            ViewBag.IssueTypeId = new SelectList(db.IssueTypes.OrderBy(p => p.Name), "Id", "Name", ticket.IssueTypeId);

            var users = db.Users.Where(z => z.IsGuest == false).Select(p => new { p.Id, p.DisplayName }).OrderBy(j => j.DisplayName).ToList();
            ViewBag.LeaderId = new SelectList(users, "Id", "DisplayName", ticket.LeaderId);

            ViewBag.PriorityId = new SelectList(db.Priorities.OrderBy(p => p.Name), "Id", "Name", ticket.PriorityId);

            ViewBag.StageId = new SelectList(db.Stages.OrderBy(p => p.Name), "Id", "Name", ticket.StageId);

            return View(ticket);
        }

        // GET: Tickets/Archive/5
        public ActionResult Archive(int? id)
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

        // POST: Tickets/Archive/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ticket ticket = db.Tickets.Find(id);
            ticket.IsArchived = true;
            db.Entry(ticket).Property(p => p.IsArchived).IsModified = true;
            db.SaveChanges();
            return RedirectToAction("Index", new { showArchived = false });
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
