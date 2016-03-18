using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TgpBugTracker.Helpers;
using TgpBugTracker.Models;

namespace TgpBugTracker.Controllers
{
    [RequireHttps]
    public class CommentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

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
            var comments = db.Comments.Where(x => x.AuthorId == user.Id || x.Ticket.AuthorId == user.Id).ToList();
            //if (comments == null)
            //    comments = new List<Comment>();
            return View(comments);
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

        // POST: Comments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,AuthorId,Date,Detail,Attachment,Title,TicketId")]
                                    Comment comment, HttpPostedFileBase upLoadFile)
        {
            if (ModelState.IsValid)
            {
                // var tHelper = new TicketsController();
                if (upLoadFile != null)
                {
                    var fileName = Path.GetFileName(upLoadFile.FileName);
                    var fullPathName = Path.Combine(Server.MapPath("/Uploads"), fileName);
                    var fHelper = new FileUpLoadValidator();
                    comment.Attachment = fHelper.SaveUpLoadFile(upLoadFile, fullPathName);
                }
                comment.Date = System.DateTimeOffset.Now;

                db.Comments.Add(comment);
                db.SaveChanges();

                /* vvvvvvvv Notify Response Leader vvvvvvvv */
                var ticket = db.Tickets.Find(comment.TicketId);
                if (comment.AuthorId != ticket.LeaderId)  // Don't  notify, if leader has generated the comment
                {
                    var notifier = db.Users.Find(User.Identity.GetUserId());
                    var nHelper = new Notifier();
                    var leader = db.Users.Find(ticket.LeaderId);
                    var ProjectName = db.Projects.Find(ticket.ProjectId).Name;
                    string[] msgElements = new string[] {"Topic: <u>New COMMENT on Ticket</u>",
                        "Comment Title: <b>" +comment.Title+"</b>",
                        "Ticket's Title: <b>" +ticket.Title+"</b>",
                        "For Project: <b>" +ProjectName+"</b>"};
                    var sentEMail = nHelper.NotifyViaEMail(leader, ticket, msgElements, notifier);
                    Debug.WriteLine("Notification sent to {0} of new comment? {1}",
                        leader.DisplayName, sentEMail);
                }
                /* ^^^^^^^ ^^^^^^^ ^^^^^^^ */

                return RedirectToAction("Details", "Tickets", new { id = comment.TicketId });
            }

            return View(comment);
        }


        // POST: Comments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "Id,AuthorId,Date,Detail,Attachment,TicketId,Title")] Comment comment)
        public ActionResult Edit([Bind(Include = "Id,Detail,Attachment,TicketId,Title")]
        Comment comment, HttpPostedFileBase upLoadFile)
        {
            if (ModelState.IsValid)
            {
                var fileName = Path.GetFileName(upLoadFile.FileName);
                var fullPathName = Path.Combine(Server.MapPath("/Uploads"), fileName);
                var fHelper = new FileUpLoadValidator();
                comment.Attachment = fHelper.SaveUpLoadFile(upLoadFile, fullPathName);

                db.Update(comment, "Title", "Detail", "Attachment");

                db.SaveChanges();

                /* vvvvvvvv Notify Response Leader vvvvvvvv */
                var ticket = db.Tickets.Find(comment.TicketId);
                var notifier = db.Users.Find(User.Identity.GetUserId());
                if (notifier.Id != ticket.LeaderId)  // Don't  notify, if leader has edit the comment
                {
                    var nHelper = new Notifier();
                    var leader = db.Users.Find(ticket.LeaderId);
                    var ProjectName = db.Projects.Find(ticket.ProjectId).Name;
                    string[] msgElements = new string[] {"Topic: <u>Comment has been EDITED on Ticket</u>",
                        "Comment Title: <b>" +comment.Title+"</b>",
                        "Ticket's Title: <b>" +ticket.Title+"</b>",
                        "For Project: <b>" +ProjectName+"</b>"};
                    var sentEMail = nHelper.NotifyViaEMail(leader, ticket, msgElements, notifier);
                    Debug.WriteLine("Notification sent to {0} of new comment? {1}",
                        leader.DisplayName, sentEMail);
                }
                /* ^^^^^^^ ^^^^^^^ ^^^^^^^ */

                return RedirectToAction("Details", "Tickets", new { id = comment.TicketId });
            }
            var errors = ModelState.Values.SelectMany(v => v.Errors);

            return View("Index", "Tickets", new { archivedOnly = false });
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
