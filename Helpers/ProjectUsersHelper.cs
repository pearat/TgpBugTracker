using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TgpBugTracker.Models;

namespace TgpBugTracker.Helpers
{
    [RequireHttps]
    public class ProjectUsersHelper
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private UserManager<ApplicationUser> manager =
            new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));

        public bool DoesProjectIncludeUser(string userId, string ProjectName)
        {
            var project = db.Projects.FirstOrDefault(p => p.Name == ProjectName);
            if (project == null)
            {
                Debug.WriteLine("DoesProjectIncludeUser() error: Project not found!");
                return false;
            }
            return (project.Users.FirstOrDefault(u => u.Id == userId) != null) ? true : false;
        }

        public IList<IdDisplayName> ListProjectUsersIds(int ProjectId)
        {
            var project = db.Projects.Find(ProjectId);
            if (project == null)
            {
                Debug.WriteLine("ListProjectUsers() error: Users not found!");
                return null;
            }
            var users = project.Users.OrderBy(p => p.DisplayName).ToArray();

            if (users == null)
            {
                Debug.WriteLine("ListProjectUsers() error: Project has no Users!");
                return null;
            }
            IList<IdDisplayName> userList = new List<IdDisplayName>();

            foreach (var item in users)
            {
                var u = new IdDisplayName();
                u.Id = item.Id;
                u.DisplayName = item.DisplayName;
                userList.Add(u);

            }
            return userList;
        }

        public IList<string> ListProjectUsers(int ProjectId)
        {
            var project = db.Projects.Find(ProjectId);
            if (project == null)
            {
                Debug.WriteLine("ListProjectUsers() error: Users not found!");
                return null;
            }
            var users = (List<string>)project.Users.OrderBy(q => q.DisplayName).Select(p => p.DisplayName ).ToList();
            if (users == null)
            {
                Debug.WriteLine("ListProjectUsers() error: Project has no Users!");
                return null;
            }
            return users;
        }


        public IList<Project> ListProjectsForUser(string userId)
        {
            if (String.IsNullOrEmpty(userId))
            {
                Debug.WriteLine("ListUserProjects() error: (\"{0}\") is either null nor empty", userId);
                return null;
            }
            var user = db.Users.Find(userId);
            if (user == null)
            {
                Debug.WriteLine("ListUserProjects() error: user oject not found");
                return null;
            }

            var projects = user.Projects.OrderBy(p => p.Name).ToList();

            if (projects == null || projects.Count() == 0)
            {
                Debug.WriteLine("ListUserProjects() error: User has no projects!");
                return null;
            }

            return projects;
        }


        public IList<Ticket> ListTicketsForUser(string userId)
        {
            if (String.IsNullOrEmpty(userId))
            {
                System.Diagnostics.Debug.WriteLine("ListTicketsForUser() error: (\"{0}\") is either null nor empty", userId);
                return null;
            }
            var user = db.Users.Find(userId);
            if (user == null)
            {
                System.Diagnostics.Debug.WriteLine("ListTicketsForUser() error: user oject not found");
                return null;
            }
            var rHelper = new UserRolesHelper();
            var UserLevel = rHelper.GetRoleRank(userId);
            var TicketList = new List<Ticket>();
            switch (UserLevel)
            {
                case (int)UserRolesHelper.RoleRank.Admin:
                    TicketList = db.Tickets.ToList();
                    //OrderBy(n => n.Project.Name).ThenBy(d => d.Date).ToList();
                    break;
                case (int)UserRolesHelper.RoleRank.PjtMgr:
                    TicketList = (List<Ticket>)(user.Projects.SelectMany(p => p.Tickets)).ToList();
                    // OrderBy(x => x.Project.Name).ThenBy(y => y.Date).ToList();
                    //TicketList = (List<Ticket>)ProjectList.SelectMany(p => p.Tickets).
                    //    OrderBy(x => x.Project.Name).ThenBy(y => y.Date).ToList();
                    break;
                case (int)UserRolesHelper.RoleRank.Developer:
                    TicketList = db.Tickets.Where(
                        t => t.LeaderId == userId ||
                        t.Project.Users.Any(i=>i.Id== userId)
                        ).ToList();
                    
                    break;
                case (int)UserRolesHelper.RoleRank.Submitter:
                    TicketList = db.Tickets.Where(t => t.AuthorId == userId).ToList();
                    break;
                default:
                    break;
            }

            TicketList = TicketList.OrderBy(n => n.Project.Name).ThenBy(d => d.Date).ToList();
            return TicketList;
        }



        public bool AddUserToProject(string ProjectName, string UserDisplayName)
        {
            var user = db.Users.FirstOrDefault(u => u.DisplayName == UserDisplayName);
            // var user = db.Users.Find(userId);
            if (user == null)
            {
                Debug.WriteLine("AddUserToProject() error: User not found.");
                return false;
            }
            var project = db.Projects.FirstOrDefault(p => p.Name == ProjectName);
            if (project == null)
            {
                Debug.WriteLine("AddUserToProject() error: Project not found.");
                return false;
            }
            if (DoesProjectIncludeUser(user.Id, ProjectName))
            {
                Debug.WriteLine("AddUserToProject() info: Project already includes this User.");
                return true;
            }
            project.Users.Add(user);
            db.SaveChanges();
            return true;        // result.Succeeded;
        }

        public bool RemoveUserFromProject(string userId, string ProjectName)
        {
            if (!DoesProjectIncludeUser(userId, ProjectName))
            {
                Debug.WriteLine("RemoveUserFromProject() info: Project didn't include User.");
                return true;
            }
            var project = db.Projects.FirstOrDefault(p => p.Name == ProjectName);
            if (project == null)
            {
                Debug.WriteLine("RemoveUserFromProject() error: Project not found.");
                return false;
            }
            var user = db.Users.Find(userId);
            if (user == null)
            {
                Debug.WriteLine("RemoveUserFromProject() error: User not found.");
                return false;
            }
            project.Users.Remove(user);
            db.SaveChanges();

            return true;        // result.Succeeded;
        }


        public IList<ApplicationUser> UsersInProject(string ProjectName)
        {
            // var db = new ApplicationDbContext();
            var usersList = new List<ApplicationUser>();
            var ProjectId = db.Projects.FirstOrDefault(n => n.Name == ProjectName).Id;
            usersList = (from u in db.Users
                         from r in u.Projects
                         where r.Id == ProjectId
                         select u).
                         ToList();
            return usersList;
        }


        public IList<ApplicationUser> UsersNotInProject(string ProjectName)
        {
            // var db = new ApplicationDbContext();
            var usersList = new List<ApplicationUser>();
            var ProjectId = db.Projects.FirstOrDefault(n => n.Name == ProjectName).Id;
            usersList = (from u in db.Users
                         from r in u.Projects
                         where r.Id != ProjectId
                         select u).
                         ToList();
            return usersList;
        }
    }
}