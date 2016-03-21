using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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

        public ActionResult Dashboard()
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            var dashboard = new DashboardVM();
            var today = System.DateTimeOffset.Now;

            // dashboard.Tickets = db.Tickets.Where(t=>t.Project .Intersect(user.Projects));
            var rHelper = new UserRolesHelper();
            var UserRank = rHelper.GetRoleRank(user.Id);
            var ProjectList = new List<Project>();
            
            if (UserRank == (int)UserRolesHelper.RoleRank.Admin)
            {
                ProjectList = db.Projects.OrderBy(n => n.Name).ToList();
                dashboard.Team = db.Users.OrderBy(y => y.LastName).ToList();
            }
            else
            {
                ProjectList = user.Projects.OrderBy(n => n.Name).ToList();
                dashboard.Team = user.Projects.SelectMany(w => w.Users).Distinct().OrderBy(y => y.LastName).ToList();
            }

            var mgrId = db.Roles.FirstOrDefault(r => r.Name == "Project Manager").Id;
            var phases = db.Phases.OrderBy(f => f.Step).ToArray();
            double x;
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
                x = (today - p.Started).TotalDays / (p.Deadline - p.Started).TotalDays;

                if (x < 0 || x > 1)
                {
                    progress.ProgressPct = 0;
                    progress.Phase = "Unknown";
                }
                else
                {
                    progress.ProgressPct = Math.Round(x, 2);
                    int xi = Convert.ToInt32(x * 100);
                    progress.Phase = phases.FirstOrDefault(q => q.Step >= xi).Name;
                    if (progress.Phase == null)
                        progress.Phase = "Fix-me!";
                }
                dashboard.Pjts.Add(progress);
            }
            
            dashboard.Tkts = ProjectList.SelectMany(p => p.Tickets).OrderByDescending(q => q.Date).ToList();
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

            dashboard.Cmts = dashboard.Tkts.SelectMany(p => p.Comments).OrderByDescending(q=>q.Date).ToList();

            ViewBag.Message = "Your Dashboard.";

            return View(dashboard);
        }
    }
}
//var possibleTeamMembers = db.Users.Where( u =>  u.Roles.Any(z => z.RoleId == devId )