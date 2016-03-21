using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using TgpBugTracker.Helpers;
using TgpBugTracker.Models;

namespace TgpBugTracker.Controllers
{
    [RequireHttps]
    [Authorize]
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Tickets
        [Authorize]
        public ActionResult Index(bool? showArchived)
        {
            var userId = User.Identity.GetUserId();
            if (userId == null)
            {
                ViewBag.FullName = "Pls login";
                ViewBag.Greeting = "Hi, ???";
                return RedirectToAction("Login", "Home");
            }

            var user = db.Users.Find(userId);
            ViewBag.FullName = user.FullName;
            ViewBag.Greeting = user.Greeting;

            var uHelper = new ProjectUsersHelper();
            var rHelper = new UserRolesHelper();
            var UserRank = rHelper.GetRoleRank(user.Id);

            ViewBag.SubmitterOnly = (UserRank == (int)UserRolesHelper.RoleRank.Submitter) ? true : false;
            ViewBag.UserId = user.Id;
            showArchived = (showArchived == null) ? false : true;
            var TicketList = uHelper.ListTicketsForUser(ViewBag.UserId, (bool)showArchived);
            ViewBag.ShowingArchived = showArchived;
            if (TicketList == null)
            {
                TicketList = new List<Ticket>();
                return View();
            }
            else
                return View(TicketList);
        }


        // GET: Tickets/Log/5
        public ActionResult Log(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            bool NeedToSaveChanges = false;
            foreach (var log in ticket.Logs)
            {
                if (log.AuthorName == null)
                {
                    log.AuthorName = db.Users.Find(log.AuthorId).DisplayName;
                    NeedToSaveChanges = true;
                }
            }
            if (NeedToSaveChanges)
                db.SaveChanges();
            ViewBag.UserId = User.Identity.GetUserId();
            return View(ticket);
        }


        // GET: Tickets/Create
        public ActionResult Create()
        {
            var tkt = new Ticket();
            tkt.AuthorId = User.Identity.GetUserId();
            tkt.Date = System.DateTimeOffset.Now;
            tkt.Deadline = tkt.Date.AddMonths(1);
            var user = db.Users.Find(User.Identity.GetUserId());
            var uHelper = new ProjectUsersHelper();
            var ProjectList = uHelper.ListProjectsForUser(user.Id);
            if (ProjectList == null)
                return RedirectToAction("Index", "Home");
            ViewBag.ProjectId = new SelectList(ProjectList, "Id", "Name");

            int defaultIssue = (int)db.IssueTypes.
                Where(z => z.Name == "Unassessed").
                Select(p => p.Id).FirstOrDefault();     // note: Where() returns a list, so need FirstOrDefault() to get 1 object
            ViewBag.IssueTypeId = new SelectList(db.IssueTypes, "Id", "Name", defaultIssue);
            TempData["defaultIssue"] = defaultIssue;



            var devId = db.Roles.FirstOrDefault(r => r.Name == "Developer").Id;
            var unassignedId = db.Roles.FirstOrDefault(r => r.Name == "Unassigned").Id;
            var possibleTeamMembers = db.Users.Where(
                u => u.IsGuest == false &&
                u.Roles.Any(z => z.RoleId == devId || z.RoleId == unassignedId)
            ).Select(p => new { p.Id, p.DisplayName });



            string defaultLeader = db.Users.Where(z => z.LastName == "Unassigned").Select(p => p.Id).FirstOrDefault();

            // var users = db.Users.Where(z => z.IsGuest == false).Select(p => new { p.Id, p.DisplayName });
            ViewBag.LeaderId = new SelectList(possibleTeamMembers, "Id", "DisplayName", defaultLeader);
            TempData["defaultLeader"] = defaultLeader;

            int defaultPriority = (int)db.Priorities.Where(z => z.Name == "Unassessed").Select(p => p.Id).FirstOrDefault();
            ViewBag.PriorityId = new SelectList(db.Priorities, "Id", "Name", defaultPriority);
            TempData["defaultPriority"] = defaultPriority;

            int defaultStage = (int)db.Stages.Where(z => z.Name == "Unassigned").Select(p => p.Id).FirstOrDefault();
            ViewBag.StageId = new SelectList(db.Stages, "Id", "Name", defaultStage);
            TempData["defaultStage"] = defaultStage;

            return View(tkt);
        }

        public string SaveUpLoadFileZ(HttpPostedFileBase upLoadFile)
        {
            if (upLoadFile != null)
            {
                var fileName = Path.GetFileName(upLoadFile.FileName);
                var OkFileName = Regex.IsMatch(fileName, "[a-zA-Z0-9]{1,200}\\.[a-zA-Z0-9]{1,10}", RegexOptions.IgnoreCase);
                if (OkFileName)
                {
                    var extension = Path.GetExtension(upLoadFile.FileName);
                    var docFileType = (extension == ".pdf" || extension == ".doc" || extension == ".docx" ||
                                        extension == ".rtf" || extension == ".txt");
                    var validImage = FileUpLoadValidator.IsWebFriendlyImage(upLoadFile);

                    if (validImage || docFileType)
                    {
                        // code for saving the image file to a physical location.
                        var fullPathName = Path.Combine(Server.MapPath("/Uploads"), fileName);
                        try
                        {
                            upLoadFile.SaveAs(fullPathName);
                        }
                        catch (Exception e)
                        {
                            fileName = "Tried to save[" + fullPathName + ", but this error occurred :" + e;
                            Debug.WriteLine(fileName);
                            return fileName;

                        }

                        if (docFileType)
                        {
                            // test whether extension and mimeType are consistent
                            var mimeType = FileUpLoadValidator.getMimeFromFile(fullPathName);
                            Debug.WriteLine("MimeType: " + mimeType);
                        }
                        // prepare a relative path to be stored in the database and used to display later on.
                        // ticket.Attachment = 


                        return "/Uploads/" + fileName;
                    }
                }
                else
                {
                    return "Problem with file name [[" + fileName + "]]";
                }
            }
            return null;
        }




        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include =
                "Id,Date,Deadline,Description,Attachment,Title,AuthorId,IssueTypeId,LeaderId,PriorityId,ProjectId,StageId")]
                Ticket ticket, HttpPostedFileBase upLoadFile)
        {
            if (ModelState.IsValid)
            {
                if (upLoadFile != null)
                {
                    var fileName = Path.GetFileName(upLoadFile.FileName);
                    var path = Server.MapPath("~/Uploads/");
                    var fullPathName = Path.Combine(path, fileName);
                    var fHelper = new FileUpLoadValidator();
                    ticket.Attachment = fHelper.SaveUpLoadFile(upLoadFile, fullPathName);
                }
                if (ticket.IssueTypeId == 0)
                    ticket.IssueTypeId = Convert.ToInt32(TempData["defaultIssue"]);

                var notifyLeader = false;
                if (ticket.LeaderId == null)
                {
                    ticket.LeaderId = (string)TempData["defaultLeader"];
                }
                else
                    notifyLeader = true;

                if (ticket.PriorityId == 0)
                    ticket.PriorityId = Convert.ToInt32(TempData["defaultPriority"]);

                if (ticket.StageId == 0)
                    ticket.StageId = Convert.ToInt32(TempData["defaultStage"]);

                db.Tickets.Add(ticket);
                db.SaveChanges();

                if (notifyLeader)
                {
                    /* vvvvvvvv Notify Response Leader vvvvvvvv */
                    if (ticket.AuthorId != ticket.LeaderId)  // Don't  notify, if leader has generated the ticket
                    {
                        var notifier = db.Users.Find(ticket.AuthorId);
                        var nHelper = new Notifier();
                        var leader = db.Users.Find(ticket.LeaderId);

                        var ProjectName = db.Projects.Find(ticket.ProjectId).Name;
                        string[] msgElements = new string[] {"Topic: <u>Assignment to New Ticket</u>",
                        "Ticket's Title: <b>" +ticket.Title+"</b>",
                        "For Project: <b>"+ProjectName+"</b>"};

                        var sentEMail = nHelper.NotifyViaEMail(leader, ticket, msgElements, notifier);

                        Debug.WriteLine("Notification sent to {0} that he/she will lead this ticket? {1}",
                            leader.DisplayName, sentEMail);
                    }
                    /* ^^^^^^^ ^^^^^^^ ^^^^^^^ */
                }

                return RedirectToAction("Index");
            }
            /*
            { "The INSERT statement conflicted with the FOREIGN KEY constraint \"FK_dbo.Tickets_dbo.IssueTypes_IssueTypeId\". 
            The conflict occurred in database \"tpeara-bugtracker\", table \"dbo.IssueTypes\", column 'Id'.
            \r\nThe statement has been terminated."}
            */
            ViewBag.ProjectId = new SelectList(db.Projects.OrderBy(n => n.Name), "Id", "Name");

            var defaultIssue = (int)db.IssueTypes.
                Where(z => z.Name == "Unassigned").
                Select(p => p.Id).FirstOrDefault();     // note: Where() returns a list, so need FirstOrDefault() to get 1 object

            ViewBag.IssueTypeId = new SelectList(db.IssueTypes, "Id", "Name", defaultIssue);

            var users = db.Users.Where(z => z.IsGuest == false).Select(p => new { p.Id, p.DisplayName }).OrderBy(n => n.DisplayName);
            ViewBag.LeaderId = new SelectList(users, "Id", "DisplayName");

            var defaultPriority = (int)db.Priorities.Where(z => z.Name == "Unassessed").Select(p => p.Id).FirstOrDefault();
            ViewBag.PriorityId = new SelectList(db.Priorities, "Id", "Name", defaultPriority);

            var defaultStage = (int)db.Stages.Where(z => z.Name == "Unassigned").Select(p => p.Id).FirstOrDefault();
            ViewBag.StageId = new SelectList(db.Stages, "Id", "Name", defaultStage);


            return View(ticket);
        }


        // GET: Tickets/Details/5
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Ticket ticket = db.Tickets.Find(id);
        //    if (ticket == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.UserId = User.Identity.GetUserId();
        //    return View(ticket);
        //}

        // GET: Tickets/Details + Edit
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserId = User.Identity.GetUserId();
            var project = db.Projects.Find(ticket.ProjectId);
            var devId = db.Roles.FirstOrDefault(r => r.Name == "Developer").Id;
            var unassignedId = db.Roles.FirstOrDefault(r => r.Name == "Unassigned").Id;
            var possibleTeamMembers = project.Users.Where(
                u => u.IsGuest == false &&
                u.Roles.Any(z => z.RoleId == devId || z.RoleId == unassignedId)
            );

            ViewBag.LeaderId = new SelectList(possibleTeamMembers, "Id", "DisplayName", ticket.LeaderId);

            ViewBag.IssueTypeId = new SelectList(db.IssueTypes.OrderBy(p => p.Name), "Id", "Name", ticket.IssueTypeId);

            ViewBag.PriorityId = new SelectList(db.Priorities.OrderBy(p => p.Name), "Id", "Name", ticket.PriorityId);

            ViewBag.StageId = new SelectList(db.Stages.OrderBy(p => p.Name), "Id", "Name", ticket.StageId);
            var rHelper = new UserRolesHelper();
            var user = db.Users.Find(User.Identity.GetUserId());
            var UserRank = rHelper.GetRoleRank(user.Id);
            ViewBag.SubmitterOnly = (UserRank == (int)UserRolesHelper.RoleRank.Submitter) ? true : false;

            return View(ticket);
        }


        // GET: Tickets/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }

            var project = db.Projects.Find(ticket.ProjectId);
            var devId = db.Roles.FirstOrDefault(r => r.Name == "Developer").Id;
            var unassignedId = db.Roles.FirstOrDefault(r => r.Name == "Unassigned").Id;
            var possibleTeamMembers = project.Users.Where(
                u => u.IsGuest == false &&
                u.Roles.Any(z => z.RoleId == devId || z.RoleId == unassignedId)
            );

            ViewBag.LeaderId = new SelectList(possibleTeamMembers, "Id", "DisplayName", ticket.LeaderId);

            ViewBag.IssueTypeId = new SelectList(db.IssueTypes.OrderBy(p => p.Name), "Id", "Name", ticket.IssueTypeId);

            ViewBag.PriorityId = new SelectList(db.Priorities.OrderBy(p => p.Name), "Id", "Name", ticket.PriorityId);


            ViewBag.StageId = new SelectList(db.Stages.OrderBy(p => p.Name), "Id", "Name", ticket.StageId);
            var rHelper = new UserRolesHelper();
            var user = db.Users.Find(User.Identity.GetUserId());
            var UserRank = rHelper.GetRoleRank(user.Id);
            ViewBag.SubmitterOnly = (UserRank == (int)UserRolesHelper.RoleRank.Submitter) ? true : false;
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include =
            "Id,Date,Deadline,Description,Attachment,Title,AuthorId,IssueTypeId,LeaderId,PriorityId,ProjectId,StageId")]
            Ticket ticket, HttpPostedFileBase upLoadFile)
        {
            if (ModelState.IsValid)
            {
                var oldTicket = db.Tickets.AsNoTracking().FirstOrDefault(t => t.Id == ticket.Id);

                if (upLoadFile != null)
                {
                    var fileName = Path.GetFileName(upLoadFile.FileName);
                    var fullPathName = Path.Combine(Server.MapPath("/Uploads"), fileName);
                    var fHelper = new FileUpLoadValidator();
                    ticket.Attachment = fHelper.SaveUpLoadFile(upLoadFile, fullPathName);
                    // SaveUpLoadFileZ(upLoadFile);
                }

                db.Entry(ticket).State = EntityState.Modified;
                db.SaveChanges();
                ticket = db.Tickets.Include("IssueType").Include("Leader").Include("Priority").Include("Project").Include("Stage").FirstOrDefault(t => t.Id == ticket.Id);
                GenerateChangeLog(oldTicket, ticket);


                /* vvvvvvvv Notify Response Leader vvvvvvvv */
                var notifier = db.Users.Find(User.Identity.GetUserId());
                if (notifier.Id != ticket.LeaderId)  // Don't  notify, if leader has modified the ticket
                {
                    var nHelper = new Notifier();
                    var ProjectName = db.Projects.Find(ticket.ProjectId).Name;
                    var leader = db.Users.Find(ticket.LeaderId);
                    if (oldTicket.LeaderId != ticket.LeaderId)
                    {
                        var oldLeader = db.Users.Find(oldTicket.LeaderId);
                        string[] msg = new string[] {"Topic: <u>Ticket Reassignment</u>",
                            "Ticket's Title: <b>" +oldTicket.Title+"</b>",
                            "For Project: <b>"+ProjectName+"</b>",
                            "Has been reassigned to: <b>"+leader.DisplayName+"."};
                        var reassignEMail = nHelper.NotifyViaEMail(oldLeader, oldTicket, msg, notifier);
                        Debug.WriteLine("Notification sent to {0} that he/she will NO LONGER lead this ticket? {1}",
                            oldLeader.DisplayName, reassignEMail);
                    }

                    string[] msgElements = new string[] {"Topic: <u>Changes to Existing Ticket</u>",
                        "Ticket's Title: <b>" +ticket.Title+"</b>",
                        "For Project: <b>"+ProjectName+"</b>"};

                    var sentEMail = nHelper.NotifyViaEMail(leader, ticket, msgElements,notifier);
                    Debug.WriteLine("Notification sent to {0} that he/she will lead this ticket? {1}",
                        leader.DisplayName, sentEMail);
                }
                /* ^^^^^^^ ^^^^^^^ ^^^^^^^ */

                // return RedirectToAction("Index");
                return RedirectToAction("Details", "Tickets", new { id = ticket.Id });
                // return View();
            }

            ViewBag.IssueTypeId = new SelectList(db.IssueTypes.OrderBy(p => p.Name), "Id", "Name", ticket.IssueTypeId);

            var users = db.Users.Where(z => z.IsGuest == false).Select(p => new { p.Id, p.DisplayName }).OrderBy(j => j.DisplayName).ToList();
            ViewBag.LeaderId = new SelectList(users, "Id", "DisplayName", ticket.LeaderId);

            ViewBag.PriorityId = new SelectList(db.Priorities.OrderBy(p => p.Name), "Id", "Name", ticket.PriorityId);

            ViewBag.StageId = new SelectList(db.Stages.OrderBy(p => p.Name), "Id", "Name", ticket.StageId);

            return View(ticket);
        }

        public void GenerateChangeLog(Ticket oldTkt, Ticket newTkt)
        {
            var today = System.DateTimeOffset.Now;
            int id = 0;
            if (newTkt.ProjectId == 0)
            {
                Debug.WriteLine("generateLog() error: new ProjectId equals 0");
                return;
            }
            if (newTkt.ProjectId != oldTkt.ProjectId)
            {
                Debug.WriteLine("generateLog() error: new {0} & old ProjectId {1} do not match ", newTkt.ProjectId, oldTkt.ProjectId);
                return;
            }

            if (newTkt.AuthorId == null)
            {
                Debug.WriteLine("generateLog() error: new AuthorId is null");
                return;
            }
            if (newTkt.AuthorId != oldTkt.AuthorId)
            {
                Debug.WriteLine("generateLog() error: new {0} & old AuthorId {1} do not match ", newTkt.AuthorId, oldTkt.AuthorId);
                return;
            }
            string authorName = db.Users.Find(User.Identity.GetUserId()).DisplayName;

            if (oldTkt?.Deadline != newTkt.Deadline)
            {
                var log0 = new Log
                {
                    Date = today,
                    AuthorName = authorName,
                    AuthorId = newTkt.AuthorId,
                    TicketId = newTkt.Id,
                    NewValue = String.Format("{0:d/M/yyyy HH:mm:ss}", newTkt.Deadline),
                    OldValue = String.Format("{0:d/M/yyyy HH:mm:ss}", oldTkt?.Deadline),
                    Property = "Deadline"
                };
                db.Logs.Add(log0);
            }

            if (oldTkt?.Description != newTkt.Description)
            {
                var log1 = new Log
                {
                    Date = today,
                    AuthorName = authorName,
                    AuthorId = newTkt.AuthorId,
                    TicketId = newTkt.Id,
                    NewValue = newTkt.Description,
                    OldValue = oldTkt?.Description,
                    Property = "Description"
                };
                db.Logs.Add(log1);
            }

            if (oldTkt?.IsArchived != newTkt.IsArchived)
            {
                var log2 = new Log
                {
                    Date = today,
                    AuthorName = authorName,
                    AuthorId = newTkt.AuthorId,
                    TicketId = newTkt.Id,
                    NewValue = newTkt.IsArchived.ToString(),
                    OldValue = oldTkt?.IsArchived.ToString(),
                    Property = "IsArchived"
                };
                db.Logs.Add(log2);
            }

            if (oldTkt?.Attachment != newTkt.Attachment)
            {
                var log3 = new Log
                {
                    Date = today,
                    AuthorName = authorName,
                    AuthorId = newTkt.AuthorId,
                    TicketId = newTkt.Id,
                    NewValue = newTkt.Attachment,
                    OldValue = oldTkt?.Attachment,
                    Property = "Attachment"
                };
                db.Logs.Add(log3);
            }

            if (oldTkt?.Title != newTkt.Title)
            {
                var log4 = new Log
                {
                    Date = today,
                    AuthorName = authorName,
                    AuthorId = newTkt.AuthorId,
                    TicketId = newTkt.Id,
                    NewValue = newTkt.Title,
                    OldValue = oldTkt?.Title,
                    Property = "Title"
                };
                db.Logs.Add(log4);
            }

            if (oldTkt?.IssueTypeId != newTkt.IssueTypeId)
            {
                string oldIssue = "";
                if (oldTkt?.IssueTypeId == 0)
                    oldIssue = "Unassessed";
                else
                {
                    id = (int)oldTkt?.IssueTypeId;
                    oldIssue = db.IssueTypes.FirstOrDefault(u => u.Id == id).Name;
                }
                var log5 = new Log
                {
                    Date = today,
                    AuthorName = authorName,
                    AuthorId = newTkt.AuthorId,
                    TicketId = newTkt.Id,
                    NewValue = newTkt.IssueType.Name,
                    OldValue = oldIssue,
                    Property = "Issue"
                };
                db.Logs.Add(log5);
            }

            if (oldTkt?.PriorityId != newTkt.PriorityId)
            {
                string oldPriority = "";
                if (oldTkt?.PriorityId == 0)
                    oldPriority = "Unassessed";
                else
                {
                    id = (int)oldTkt?.PriorityId;
                    oldPriority = db.Priorities.FirstOrDefault(u => u.Id == id).Name;
                }
                var log6 = new Log
                {
                    Date = today,
                    AuthorName = authorName,
                    AuthorId = newTkt.AuthorId,
                    TicketId = newTkt.Id,
                    NewValue = newTkt.Priority.Name,
                    OldValue = oldPriority,
                    Property = "Priority"
                };
                db.Logs.Add(log6);
            }

            if (oldTkt?.LeaderId != newTkt.LeaderId)
            {
                string oldLeader = "";
                if (oldTkt?.LeaderId == null)
                    oldLeader = "Unassigned";
                else
                {
                    var oldLeaderId = (string)oldTkt?.LeaderId;
                    oldLeader = db.Users.FirstOrDefault(u => u.Id == oldLeaderId).DisplayName;
                }
                var log7 = new Log
                {
                    Date = today,
                    AuthorName = authorName,
                    AuthorId = newTkt.AuthorId,
                    TicketId = newTkt.Id,
                    NewValue = newTkt.Leader.DisplayName,
                    OldValue = oldLeader,
                    Property = "Leader"
                };
                db.Logs.Add(log7);
            }

            if (oldTkt?.StageId != newTkt.StageId)
            {
                string oldStage = "";
                if (oldTkt?.StageId == 0)
                    oldStage = "Unassessed";
                else
                {
                    id = (int)oldTkt?.StageId;
                    oldStage = db.Stages.FirstOrDefault(u => u.Id == id).Name;
                }
                var log8 = new Log
                {
                    Date = today,
                    AuthorName = authorName,
                    AuthorId = newTkt.AuthorId,
                    TicketId = newTkt.Id,
                    NewValue = newTkt.Stage.Name,
                    OldValue = oldStage,
                    Property = "Stage"
                };
                db.Logs.Add(log8);
            }
            db.SaveChanges();
        }


        // GET: Tickets/Archive/5
        public ActionResult Archive(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        // POST: Tickets/Archive/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ticket ticket = db.Tickets.Find(id);
            ticket.IsArchived = true;
            db.Entry(ticket).Property(p => p.IsArchived).IsModified = true;
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
