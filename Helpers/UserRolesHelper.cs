using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TgpBugTracker.Models;

namespace TgpBugTracker.Helpers
{
    [RequireHttps]
    public class UserRolesHelper
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private UserManager<ApplicationUser> manager =
            new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));

        public enum RoleRank
        {
            None = 0,
            Submitter = 20,
            Unassigned = 30,
            Developer = 40,
            PjtMgr = 80,
            Admin = 100
        }

        public bool IsUserInRole(string userId, string roleName)
        {
            
            return manager.IsInRole(userId, roleName);
        }

        public IList<string> ListUserRoles(string userId)
        {
            return manager.GetRoles(userId).ToArray();
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


        public IList<ApplicationUser> UsersInRole(string roleName)
        {
            var db = new ApplicationDbContext();
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

        public int GetRoleRank(string IdOrAuthLevel)
        {
            if (IdOrAuthLevel.Length > 15)
            {
                string userId = IdOrAuthLevel;
                if (IsUserInRole(userId, "Admin"))
                    return (int)RoleRank.Admin;

                if (IsUserInRole(userId, "Project Manager"))
                    return (int)RoleRank.PjtMgr;

                if (IsUserInRole(userId, "Developer"))
                    return (int)RoleRank.Developer;

                if (IsUserInRole(userId, "Unassigned"))
                    return (int)RoleRank.Unassigned;

                if (IsUserInRole(userId, "Submitter"))
                    return (int)RoleRank.Submitter;

                return (int)RoleRank.None;
            }

            else
            {
                string authLevel = IdOrAuthLevel;
                switch (authLevel)
                {
                    case "Admin":
                        return (int)RoleRank.Admin;
                    case "Developer":
                        return (int)RoleRank.Developer;
                    case "Project Manager":
                        return (int)RoleRank.PjtMgr;
                    case "Submitter":
                        return (int)RoleRank.Submitter;
                    case "Unassigned":
                        return (int)RoleRank.Unassigned;
                    default:
                        return (int)RoleRank.None;
                }
            }
        }

        public string GetUserAuthorizationLevel(int roleRank)
        {
            switch (roleRank)
            {
                case (int)RoleRank.Admin:
                    return "Admin";
                case (int)RoleRank.PjtMgr:
                    return "Project Manager";
                case (int)RoleRank.Developer:
                    return "Developer";
                case (int)RoleRank.Submitter:
                    return "Submitter";
                case (int)RoleRank.Unassigned:
                    return "Unassigned";
                default:
                    return "None";
            }
        }

    }
}