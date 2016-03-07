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
            // var users = project.Users.OrderBy(p => p.DisplayName).Select(p => new { p.Id, p.DisplayName } ).ToArray();
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
            var users = project.Users.OrderBy(p => p.DisplayName).Select(p => p.DisplayName).ToList();
            if (users == null)
            {
                Debug.WriteLine("ListProjectUsers() error: Project has no Users!");
                return null;
            }
            return users;
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

            //var uP = new UserProject();
            //uP.UserId = user.Id;
            //uP.ProjectId = project.Id;
            //db.UserProjects.Add(uP);
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

            //var uP = db.UserProjects.FirstOrDefault(ups => ups.UserId == user.Id && ups.ProjectId == project.Id);

            project.Users.Remove(user);
            db.SaveChanges();

            return true;        // result.Succeeded;
        }

        //public IList<string> ListAllProjects()
        //{
        //    var Projects = new List<string>();
        //    Projects = (from r in db.Projects select r.Name).ToList();
        //    return Projects;
        //}

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