using Microsoft.AspNet.Identity;
using System.Web.Mvc;
using TgpBugTracker.Models;

namespace TgpBugTracker.Helpers
{
    [RequireHttps]
    public class Notifier
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public bool NotifyViaEMail(ApplicationUser leader,Ticket ticket, string[] changes, ApplicationUser author)
        {
            var notice = new Notice();
            notice.Date = System.DateTimeOffset.Now;
            notice.AuthorId = author.Id;
            notice.LeaderId = leader.Id;
            notice.TicketId = ticket.Id;
            var SendTo = leader.Email;
            var es = new EmailService();
            string body = "Attention: <b>"+leader.FullName + "</b><br />";
            int i = 0;
            foreach (var item in changes)
            {
                body += item+".<br />";
                if (i==0)
                {
                    notice.Detail = item;
                }
                i++;
            }
            body += "Notification from: " + author.FullName + ".<br />";
            body += "<hr>This is an automatically generated message.  Please login to review the ticket details.";
            var msg = new IdentityMessage
            {
                Subject = "Notification of Ticket Assignment or Change",
                Destination = SendTo,
                Body = body
            };
            es.SendAsync(msg);
            db.Notices.Add(notice);
            db.SaveChanges();

            return true;
        }
    }
}