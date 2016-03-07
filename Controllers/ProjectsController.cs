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
    public class ProjectsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Projects
        public ActionResult Index()
        {
            var assignedProjects = new List<ProjectUsersVM>();
            var uHelper = new ProjectUsersHelper();
            var rHelper = new UserRolesHelper();
            var allUsers = db.Users.OrderBy(r => r.DisplayName).Select(r => r.DisplayName).ToArray();
            var numAllUsers = allUsers.Count();

            foreach (var p in db.Projects)
            {
                var projectVM = new ProjectUsersVM();

                projectVM.ProjectId = p.Id;
                projectVM.ProjectName = p.Name;

                var teamMembers = uHelper.ListProjectUsersIds(p.Id);
                if (teamMembers != null)
                {
                    ViewBag.TeamCount = teamMembers.Count();
                    int pmCount = 0;
                    int devCount = 0;
                    int subCount = 0;
                    projectVM.PjtMgrs = new string[ViewBag.TeamCount];
                    projectVM.Developers = new string[ViewBag.TeamCount];
                    projectVM.Submitters = new string[ViewBag.TeamCount];

                    for (int k = 0; k < ViewBag.TeamCount; k++)
                    {
                        if (rHelper.IsUserInRole(teamMembers[k].Id, "Project Manager"))
                        {
                            // var temp = db.Users.Find(teamMembers[k]);
                            projectVM.PjtMgrs[pmCount++] = teamMembers[k].DisplayName;
                        }
                        if (rHelper.IsUserInRole(teamMembers[k].Id, "Developer"))
                        {
                            //var temp = db.Users.Find(teamMembers[k]);
                            projectVM.Developers[devCount++] = teamMembers[k].DisplayName;
                        }
                        if (rHelper.IsUserInRole(teamMembers[k].Id, "Submitter"))
                        {
                            // var temp = db.Users.Find(teamMembers[k]);
                            projectVM.Submitters[subCount++] = teamMembers[k].DisplayName;
                        }
                    }
                }
                assignedProjects.Add(projectVM);
            }
            ViewBag.NumUsers = numAllUsers;
            return View(assignedProjects);
        }



        //
        // GET: /Manage/AssignUsersToProject

        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult AssignUsersToProject(int ProjectId)
        {
            // find user; create, populate and then send staff object
            // int projectId = Convert.ToInt32(ProjectId);
            var project = db.Projects.Find(ProjectId);
            if (project == null)
            {
                ModelState.AddModelError("", "Project Id not found.");
                return RedirectToAction("Index");
            }
            var team = new ProjectUsersVM();
            team.ProjectId = ProjectId;

            team.ProjectName = project.Name;
            var helper = new ProjectUsersHelper();
            var a = project.Users;
            team.Usrs = helper.ListProjectUsers(ProjectId).ToArray();
            team.Selected = new MultiSelectList(db.Users, "DisplayName", "DisplayName", team.Usrs);

            return View(team);
        }

        //
        // POST: /Manage/AssignUsersToProject
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult AssignUsersToProject([Bind(Include = "ProjectId,ProjectName,Usrs,Selected")] ProjectUsersVM Pjt)
        {
            if (!ModelState.IsValid)
            {
                return View(Pjt.ProjectId);
            }
            // (Re-)Assign users for this Project
            var helper = new ProjectUsersHelper();
            var project = db.Projects.Find(Pjt.ProjectId);
            project.Users.Clear();
            db.SaveChanges();
            int ctr = 0;
            if (Pjt.Usrs == null)
                ctr = 0;
            else
                ctr = Pjt.Usrs.Count();

            for (int i = 0; i < ctr; i++)
            {
                helper.AddUserToProject(Pjt.ProjectName, Pjt.Usrs[i]);
            }


            return RedirectToAction("Index", "Projects");
        }



        // GET: Projects/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // GET: Projects/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult Create([Bind(Include = "Id,AuthorId,Deadline,Description,Name,Started,Version")] Project project)
        {
            if (ModelState.IsValid)
            {
                db.Projects.Add(project);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include = "Id,AuthorId,Deadline,Description,Name,Started,Version")] Project project)
        {
            if (ModelState.IsValid)
            {
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(project);
        }

        // GET: Projects/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            Project project = db.Projects.Find(id);
            db.Projects.Remove(project);
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
