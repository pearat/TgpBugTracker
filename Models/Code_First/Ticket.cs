using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TgpBugTracker.Models
{
    public class Ticket
    {
        public Ticket()
        {
            this.Comments = new HashSet<Comment>();
            this.Logs = new HashSet<Log>();
            this.Notices = new HashSet<Notice>();
        }
        [Required]
        public int Id { get; set; }
        [Required]
        public DateTimeOffset Date { get; set; }
        public DateTimeOffset? Deadline { get; set; }
        [Required]
        public string Issue { get; set; }
        public string RepositoryURL { get; set; }
        public string Title { get; set; }
        [Required]
        public int AuthorId { get; set; }
        [Required]
        public int IssueTypeId { get; set; }
        public int? LeaderId { get; set; }

        [Required]
        public int PriorityId { get; set; }
        [Required]
        public int ProjectId { get; set; }
        [Required]
        public int StageId { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Log> Logs { get; set; }
        public virtual ICollection<Notice> Notices { get; set; }

    }
}