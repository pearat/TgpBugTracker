﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TgpBugTracker.Models;
using TgpBugTracker.Helpers;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace TgpBugTracker.Controllers
{
    [RequireHttps]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        //
        // GET: Admin
        public ActionResult Index()
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

            var fullStaff = new List<UserRolesVM>();
            var helper = new UserRolesHelper();
            var allRoles = db.Roles.OrderBy(r => r.Name).Select(r => r.Name).ToArray();
            var numAllRoles = allRoles.Count();
            foreach (var u in db.Users)
            {
                var staff = new UserRolesVM();
                staff.UserId = u.Id;
                staff.DisplayName = u.DisplayName;
                staff.IsGuest = u.IsGuest;

                staff.Roles = new string[numAllRoles];

                switch (u.RoleRank)
                {
                    case (int)UserRolesHelper.RoleRank.Admin: staff.Roles[0] = "A"; break;
                    case (int)UserRolesHelper.RoleRank.PjtMgr: staff.Roles[1] = "P"; break;
                    case (int)UserRolesHelper.RoleRank.Developer: staff.Roles[2] = "D"; break;
                    case (int)UserRolesHelper.RoleRank.Submitter: staff.Roles[3] = "S"; break;
                    case (int)UserRolesHelper.RoleRank.Unassigned: staff.Roles[4] = "U"; break;
                    default:
                        staff.Roles[4] = "U";
                        break;
                }

                //if (sRoles[i].Equals("Developer", StringComparison.Ordinal)) staff.Roles[2] = "D";
                //    if (sRoles[i].Equals("Submitter", StringComparison.Ordinal)) staff.Roles[3] = "S";
                //}
                fullStaff.Add(staff);
            }
            ViewBag.NumRoles = 5;
            return View(fullStaff);
        }


        //
        // GET: /Manage/AssignRoleToUser to Users
        public ActionResult AssignRoleToUser(string userId)
        {
            if (userId == null)
                userId = User.Identity.GetUserId();
            // find user; create, populate and then send staff object
            var user = db.Users.Find(userId);
            if (user == null)
            {
                ModelState.AddModelError("", "User Id not found.");
                return RedirectToAction("Index");
            }
            var staff = new UserRolesVM();
            staff.UserId = userId;
            staff.IsGuest = user.IsGuest;
            staff.DisplayName = user.DisplayName;
            var helper = new UserRolesHelper();
            staff.RoleRank = helper.GetRoleRank(userId);
            staff.AuthLevel = helper.GetUserAuthorizationLevel(staff.RoleRank);
            ViewBag.AuthLevel = new SelectList(db.Roles.OrderBy(p => p.Name), "Name", "Name", staff.AuthLevel);
            return View(staff);
        }

        //
        // POST: /Manage/AssignRoleToUser to Users
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignRoleToUser([Bind(Include = "AuthLevel,UserId,DisplayName,IsGuest,Selected,Roles")] UserRolesVM staff)
        {
            if (!ModelState.IsValid)
            {
                return View(staff.UserId);
            }

            var rHelper = new UserRolesHelper();
            var staffRoles = db.Roles.Find(staff.UserId);
            if (staffRoles != null)
                staffRoles.Users.Clear();
            db.SaveChanges();

            var user = db.Users.Find(staff.UserId);

            rHelper.AddUserToRole(staff.UserId, staff.AuthLevel);
            user.RoleRank = rHelper.GetRoleRank(staff.AuthLevel);
            user.IsGuest = staff.IsGuest;
            db.Entry(user).Property(g => g.IsGuest).IsModified = true;
            db.Entry(user).Property(g => g.RoleRank).IsModified = true;

            db.SaveChanges();

            return RedirectToAction("Index", "Admin");
        }

    }
}