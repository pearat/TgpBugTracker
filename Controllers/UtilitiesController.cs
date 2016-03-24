using Microsoft.AspNet.Identity;
using System;
using System.Diagnostics;
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
            Utils.Issues = db.IssueTypes.OrderBy(p => p.Name).ToList();
            Utils.Phases = db.Phases.OrderBy(p=>p.Step).ToList();
            Utils.Priorities = db.Priorities.OrderBy(p => p.Name).ToList();
            Utils.Stages = db.Stages.OrderBy(p => p.Name).ToList();
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
        public ActionResult Create([Bind(Include = "Datatype,Name,Step")]UtilityVM util)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                Debug.WriteLine("errors: " + errors);
            }
            else
            {
                var administrator = db.Users.Find(User.Identity.GetUserId());
                if (!administrator.IsGuest)
                {
                    switch (util.Datatype)
                    {
                        case "Phase":
                            var existingPhase = db.Phases.FirstOrDefault(n => n.Name == util.Name);
                            var existingStep = db.Phases.FirstOrDefault(n => n.Step == util.Step);
                            if (existingPhase != null || existingStep!=null)
                                return RedirectToAction("Index");
                            var phase = new Phase();
                            phase.Name = util.Name;
                            phase.Step = util.Step;
                            db.Phases.Add(phase);
                            break;
                        case "Issue":
                            var existingIssue = db.IssueTypes.FirstOrDefault(n => n.Name == util.Name);
                            if (existingIssue != null)
                                return RedirectToAction("Index");
                            var issue = new IssueType();
                            issue.Name = util.Name;
                            db.IssueTypes.Add(issue);
                            
                            break;
                        case "Priority":
                            var existingPriority = db.Priorities.FirstOrDefault(n => n.Name == util.Name);
                            if (existingPriority != null)
                                return RedirectToAction("Index");
                            var priority = new Priority();
                            priority.Name = util.Name;
                            db.Priorities.Add(priority);
                            break;
                        case "Stage":
                            var existingStage = db.Stages.FirstOrDefault(n => n.Name == util.Name);
                            if (existingStage != null)
                                return RedirectToAction("Index");
                            var stage = new Stage();
                            stage.Name = util.Name;
                            db.Stages.Add(stage);
                            break;
                        default:
                            break;
                    }
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Datatype,Id,Name,Step")] UtilityVM util)
        {
            if (ModelState.IsValid)
            {
                var administrator = db.Users.Find(User.Identity.GetUserId());
                if (!administrator.IsGuest)
                {
                    switch (util.Datatype)
                    {
                        case "Phase":
                            var phase = db.Phases.Find(util.Id);
                            if (phase.Name == "Unassessed")
                                return RedirectToAction("Index");
                            phase.Name = util.Name;
                            phase.Step = (int)util.Step;
                            db.Update(phase, "Name", "Step");
                            break;
                        case "Issue":
                            var issue = db.IssueTypes.Find(util.Id);
                            if (issue.Name == "Unassessed")
                                return RedirectToAction("Index");
                            issue.Name = util.Name;
                            db.Update(issue, "Name");
                            break;
                        case "Priority":
                            var priority = db.Priorities.Find(util.Id);
                            if (priority.Name == "Unassessed")
                                return RedirectToAction("Index");
                            priority.Name = util.Name;
                            db.Update(priority, "Name");
                            break;
                        case "Stage":
                            var stage = db.Stages.Find(util.Id);
                            if (stage.Name == "Unassigned")
                                return RedirectToAction("Index");
                            stage.Name = util.Name;
                            db.Update(stage, "Name");
                            break;
                        default:
                            break;
                    }
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete([Bind(Include = "Datatype,Id,Name,Step")] UtilityVM util)
        {
            if (ModelState.IsValid)
            {
                var administrator = db.Users.Find(User.Identity.GetUserId());

                // Ressign any ticket with this name to Unassigned

                if (!administrator.IsGuest)
                {
                    switch (util.Datatype)
                    {
                        case "Phase":
                            var phase = db.Phases.Find(util.Id);
                            if (phase.Name == "Unassessed")
                                return RedirectToAction("Index");
                            db.Phases.Remove(phase);
                            break;
                        case "Issue":
                            var issue = db.IssueTypes.Find(util.Id);
                            var unassessedId = db.IssueTypes.FirstOrDefault(n => n.Name == "Unassessed").Id;
                            if (issue.Name == "Unassessed" || unassessedId==0)
                                return RedirectToAction("Index");
                            db.Tickets.Where(i => i.IssueTypeId == util.Id).ToList()
                                .ForEach(a => a.IssueTypeId = unassessedId);
                            db.IssueTypes.Remove(issue);
                            break;
                        case "Priority":
                            var priority = db.Priorities.Find(util.Id);
                            var unassessedId2 = db.Priorities.FirstOrDefault(n => n.Name == "Unassessed").Id;
                            if (priority.Name == "Unassessed" || unassessedId2 == 0)
                                return RedirectToAction("Index");
                            db.Tickets.Where(i => i.PriorityId == util.Id).ToList()
                                .ForEach(a => a.PriorityId = unassessedId2);
                            db.Priorities.Remove(priority);
                            break;
                        case "Stage":
                            var stage = db.Stages.Find(util.Id);
                            var unassignedId = db.Stages.FirstOrDefault(n => n.Name == "Unassigned").Id;
                            if (stage.Name == "Unassigned" || unassignedId == 0)
                                return RedirectToAction("Index");
                            db.Tickets.Where(i => i.StageId == util.Id).ToList()
                                .ForEach(a => a.StageId = unassignedId);
                            db.Stages.Remove(stage);
                            break;
                        default:
                            break;
                    }
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }

    }
}