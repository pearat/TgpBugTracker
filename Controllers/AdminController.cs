using System;
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
                var sRoles = helper.ListUserRoles(u.Id).ToArray();
                var sRc = sRoles.Count();
                for (int i = 0; i < sRc; i++)
                {
                    if (sRoles[i].Equals("Admin", StringComparison.Ordinal)) staff.Roles[0] = "A";
                    if (sRoles[i].Equals("Project Manager", StringComparison.Ordinal)) staff.Roles[1] = "P";
                    if (sRoles[i].Equals("Developer", StringComparison.Ordinal)) staff.Roles[2] = "D";
                    if (sRoles[i].Equals("Submitter", StringComparison.Ordinal)) staff.Roles[3] = "S";
                }
                fullStaff.Add(staff);
            }
            ViewBag.NumRoles = numAllRoles;
            return View(fullStaff);
        }


        //
        // GET: /Manage/AssignRolesToUser to Users
        public ActionResult AssignRolesToUser(string UserId)
        {
            if (UserId == null)
                UserId = User.Identity.GetUserId();
            // find user; create, populate and then send staff object
            var user = db.Users.Find(UserId);
            if (user == null)
            {
                ModelState.AddModelError("", "User Id not found.");
                return RedirectToAction("Index");
            }
            var staff = new UserRolesVM();
            staff.UserId = UserId;
            staff.IsGuest = user.IsGuest;
            staff.DisplayName = user.DisplayName;
            var helper = new UserRolesHelper();
            staff.Roles = helper.ListUserRoles(UserId).ToArray();
            staff.Selected = new MultiSelectList(db.Roles, "Name", "Name", staff.Roles);

            return View(staff);
        }

        //
        // POST: /Manage/AssignRolesToUser to Users
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignRolesToUser([Bind(Include = "UserId,DisplayName,IsGuest,Roles,Selected")] UserRolesVM staff)
        {
            if (!ModelState.IsValid)
            {
                return View(staff.UserId);
            }
            // (Re-)Assign roles for this user
            var helper = new UserRolesHelper();
            foreach (var r in db.Roles)
            {
                if (helper.IsUserInRole(staff.UserId, r.Name))
                    helper.RemoveUserFromRole(staff.UserId, r.Name);
            }
            for (int i = 0; i < staff.Roles.Count(); i++)
            {
                helper.AddUserToRole(staff.UserId, staff.Roles[i]);
            }
            var user = db.Users.Find(staff.UserId);
            user.IsGuest = staff.IsGuest;
            db.Entry(user).Property(g => g.IsGuest).IsModified = true;
            db.SaveChanges();

            return RedirectToAction("Index", "Admin");
        }

    }
}