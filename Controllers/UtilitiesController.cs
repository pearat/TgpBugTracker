using Microsoft.AspNet.Identity;
using System.Linq;
using System.Web.Mvc;
using TgpBugTracker.Helpers;
using TgpBugTracker.Models;

namespace TgpBugTracker.Controllers
{
    [RequireHttps]
    [Authorize(Roles = "Admin")]
    public class UtilitiesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Utilities
        public ActionResult Index()
        {
            var Utils = new UtilitiesVM();
            Utils.Issues = db.IssueTypes.ToList();
            Utils.Phases = db.Phases.ToList();
            Utils.Priorities = db.Priorities.ToList();
            Utils.Stages = db.Stages.ToList();
            return View(Utils);
        }


        //
        // POST: /Manage/AssignRoleToUser to Users
        [HttpPost]
        public ActionResult ResetGuestData()
        {
            var administrator = db.Users.Find(User.Identity.GetUserId());
            if (!administrator.IsGuest)
                db.Database.ExecuteSqlCommand("EXEC PrepareGuestData");
            return RedirectToAction("Index");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "Id,AuthorId,Date,Detail,Attachment,TicketId,Title")] Comment comment)
        public ActionResult EditStage([Bind(Include = "Id,Name")] Stage stage)
        {
            if (ModelState.IsValid)
            {
                var administrator = db.Users.Find(User.Identity.GetUserId());
                if (!administrator.IsGuest)
                {
                    db.Update(stage, "Name");
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "Id,AuthorId,Date,Detail,Attachment,TicketId,Title")] Comment comment)
        public ActionResult DeleteStage([Bind(Include = "Id,Name")] Stage stage)
        {
            if (ModelState.IsValid)
            {
                var administrator = db.Users.Find(User.Identity.GetUserId());
                if (!administrator.IsGuest)
                {
                    // Can't delete Unassigned!
                    // Ressign any ticket with this name to Unassigned

                    //db.Update(stage, "Name");
                    //db.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }

    }
}