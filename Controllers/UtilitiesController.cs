using System.Linq;
using System.Web.Mvc;
using TgpBugTracker.Models;

namespace TgpBugTracker.Controllers
{
    [RequireHttps]
    [Authorize(Roles = "Admin")]
    public class UtilitiesController : Controller
    {
         private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Utilities
        public ActionResult Index()
        {
            var Utils = new UtilitiesVM();
            Utils.Issues = db.IssueTypes.ToList();
            Utils.Phases = db.Phases.ToList();
            Utils.Priorities = db.Priorities.ToList();
            Utils.Stages = db.Stages.ToList();
            return View(Utils);
        }
    }
}