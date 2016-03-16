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
    }
}