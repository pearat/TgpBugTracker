using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TgpBugTracker.Models;

namespace TgpBugTracker.Controllers
{
    [RequireHttps]
    public class CommentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: _Comments
        public ActionResult _Comments(int TktId)
        {
            var ticket = db.Tickets.Find(TktId);
            var comments = ticket.Comments.OrderByDescending(c => c.Date).ToList();
            if (comments == null)
            {
                comments = new List<Comment>();
            }
            ViewBag.TktId = TktId;
            return PartialView("_Comments",comments);
        }

        // GET: Comments
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

            return View(db.Comments.ToList());
        }

        // GET: Comments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        // GET: Comments/Create
        //public ActionResult Create(int? TktId)
        //{
        //    if (TktId==null)
        //    {
        //        Debug.WriteLine("Comments/Create() error: Ticket Id is null!");
        //        return RedirectToAction("View", "Comments");
        //    }
        //    Comment comment = new Comment();
        //    var user = db.Users.Find(User.Identity.GetUserId());
        //    comment.AuthorId = user.Id;
        //    comment.Date= System.DateTimeOffset.Now;
        //    comment.TicketId = (int)TktId;
        //    return View(comment);
        //}

        // POST: Comments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,AuthorId,Date,Detail,MediaURL,Title,TicketId")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                comment.Date = System.DateTimeOffset.Now;
                
                db.Comments.Add(comment);
                db.SaveChanges();
                return RedirectToAction("Details", "Tickets", new { id = comment.TicketId });
            }

            return View(comment);
        }

        // GET: Comments/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Comment comment = db.Comments.Find(id);
        //    if (comment == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(comment);
        //}

        // POST: Comments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,AuthorId,Date,Detail,MediaURL,TicketId,Title")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                //var user = db.Users.Find(User.Identity.GetUserId());
                //comment.AuthorId = user.Id;
                //comment.Date = System.DateTimeOffset.Now;
                ////db.Entry(comment).State = EntityState.Modified;
                //db.SaveChanges();

                db.Comments.Attach(comment);
                db.Entry(comment).Property(p => p.Title).IsModified = true;
                db.Entry(comment).Property(p => p.Detail).IsModified = true;
                db.Entry(comment).Property(p => p.MediaURL).IsModified = true;
                db.SaveChanges();
                //return RedirectToAction("Index");
                return RedirectToAction("details", "Tickets", new { id = comment.TicketId });
            }
            return View("Index","Tickets");
        }

        // GET: Comments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Comment comment = db.Comments.Find(id);
            db.Comments.Remove(comment);
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
