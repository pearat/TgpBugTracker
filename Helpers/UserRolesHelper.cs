using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TgpBugTracker.Models;

namespace TgpBugTracker.Helpers
{
    public class UserRolesHelper
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private UserManager<ApplicationUser> manager =
            new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));

        public bool IsUserInRole(string userId, string roleName)
        {
            return manager.IsInRole(userId, roleName);
        }

        public IList<string> ListUserRoles(string userId)
        {
            return manager.GetRoles(userId);
        }

        public bool AddUserToRole(string userId, string roleName)
        {
            var result = manager.AddToRoles(userId, roleName);
            return result.Succeeded;
        }

        public bool RemoveUserFromRole(string userId, string roleName)
        {
            var result = manager.RemoveFromRole(userId, roleName);
            return result.Succeeded;
        }

        //public IList<string> ListAllRoles()
        //{
        //    var roles = new List<string>();
        //    roles = (from r in db.Roles select r.Name).ToList();
        //    return roles;
        //}

        public IList<ApplicationUser> UsersInRole(string roleName)
        {
            // var db = new ApplicationDbContext();
            var usersList = new List<ApplicationUser>();
            var roleId = db.Roles.FirstOrDefault(n => n.Name == roleName).Id;
            usersList = (from u in db.Users
                         from r in u.Roles
                         where r.RoleId == roleId
                         select u).
                         ToList();
            return usersList;
        }


        public IList<ApplicationUser> UsersNotInRole(string roleName)
        {
            // var db = new ApplicationDbContext();
            var usersList = new List<ApplicationUser>();
            var roleId = db.Roles.FirstOrDefault(n => n.Name == roleName).Id;
            usersList = (from u in db.Users
                         from r in u.Roles
                         where r.RoleId != roleId
                         select u).
                         ToList();
            return usersList;
        }
    }
}