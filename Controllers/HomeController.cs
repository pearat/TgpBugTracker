using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TgpBugTracker.Helpers;
using TgpBugTracker.Models;

namespace TgpBugTracker.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            if (userId == null)
            {
                ViewBag.FullName = "Pls login";
                ViewBag.Greeting = "Hi, ???";

            }
            else
            {
                var user = db.Users.Find(userId);
                ViewBag.FullName = user.FullName;
                ViewBag.Greeting = user.Greeting;
            }
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [Authorize]
        public ActionResult Dashboard()
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            var dashboard = new DashboardVM();
            var today = System.DateTimeOffset.Now;

            var rHelper = new UserRolesHelper();
            var UserRank = rHelper.GetRoleRank(user.Id);
            var ProjectList = new List<Project>();

            ProjectList = user.Projects.OrderBy(n => n.Name).ToList();


            var mgrId = db.Roles.FirstOrDefault(r => r.Name == "Project Manager").Id;
            var phases = db.Phases.OrderBy(f => f.Step).ToArray();
            double percentageCompleted;
            foreach (var p in ProjectList)
            {
                Progress progress = new Progress();
                progress.ProjectId = p.Id;
                progress.ProjectName = p.Name;
                progress.Version = p.Version;

                var projectMgrs = p.Users.Where(u => u.Roles.Any(z => z.RoleId == mgrId)).ToList();
                if (projectMgrs != null)
                {
                    int j = 0;
                    foreach (var pm in projectMgrs)
                    {
                        if (j++ > 0)
                            progress.Manager += "/";
                        progress.Manager += pm.LastName;
                    }
                }
                else
                    progress.Manager = "Unassigned";

                progress.Deadline = p.Deadline.LocalDateTime;
                percentageCompleted = (today - p.Started).TotalDays / (p.Deadline - p.Started).TotalDays;

                if (percentageCompleted < 0 || percentageCompleted > 1)
                {
                    progress.ProgressPct = 0;
                    progress.Phase = "Unknown";
                }
                else
                {
                    progress.ProgressPct = Math.Round(percentageCompleted, 2);
                    int amountCompleted = Convert.ToInt32(percentageCompleted * 100);
                    progress.Phase = phases.FirstOrDefault(q => q.Step >= amountCompleted).Name;
                    if (progress.Phase == null)
                        progress.Phase = "Fix-me!";
                }
                dashboard.Pjts.Add(progress);
            }

            if (UserRank == (int)UserRolesHelper.RoleRank.Submitter)
            {
                dashboard.Tkts = ProjectList.SelectMany(p => p.Tickets).
                    Where(a => a.AuthorId == user.Id).OrderByDescending(q => q.Date).ToList();
                dashboard.Cmts = dashboard.Tkts.SelectMany(p => p.Comments).
                    Where(a => a.AuthorId == user.Id).OrderByDescending(q => q.Date).ToList();

                //var subId = db.Roles.FirstOrDefault(r => r.Name == "Submitter").Id;
                //var s1 = db.Users.Where(u => u.IsGuest == user.IsGuest);
                //var s2 = s1.Where(u2 => u2.Roles.Any(z => z.RoleId == subId));
                //var s3 = s2.Distinct().OrderBy(y => y.DisplayName).ToList();
                dashboard.Team.Add(user);
            }
            else
            {
                dashboard.Tkts = ProjectList.SelectMany(p => p.Tickets).OrderByDescending(q => q.Date).ToList();
                dashboard.Cmts = dashboard.Tkts.SelectMany(p => p.Comments).OrderByDescending(q => q.Date).ToList();
                var firstCut = user.Projects.SelectMany(w => w.Users).Distinct().OrderBy(y => y.DisplayName).ToList();
                dashboard.Team = firstCut.Where(u => u.LastName != "Unassigned").ToList();
                var notices = dashboard.Tkts.SelectMany(p => p.Notices).OrderByDescending(q => q.Date).Take(8).ToList();
                foreach (var item in notices)
                {
                    var n = new NoticeVM();
                    n.Date = item.Date;
                    n.Title = item.Detail;
                    n.ProjectName = db.Tickets.Find(item.TicketId).Project.Name;
                    if (n.ProjectName == null)
                        n.ProjectName = "Check Pjt Name";
                    dashboard.Ntcs.Add(n);
                }
            }
            foreach (var item in dashboard.Tkts)
            {
                if (item.Attachment != null)
                    dashboard.AttachmentCount++;
            }
            foreach (var item in dashboard.Cmts)
            {
                if (item.Attachment != null)
                    dashboard.AttachmentCount++;
            }

            ViewBag.Message = "Your Dashboard.";

            return View(dashboard);
        }
    }
}
//var possibleTeamMembers = db.Users.Where( u =>  u.Roles.Any(z => z.RoleId == devId )