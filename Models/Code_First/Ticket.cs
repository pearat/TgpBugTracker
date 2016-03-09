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
        [DataType(DataType.Date)]
        [Display(Name = "Tkt Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
        public DateTimeOffset Date { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Deadline")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
        public DateTimeOffset? Deadline { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Description")]
        public string Description { get; set; }


        [Display(Name = "Is Archived?")]
        public bool IsArchived { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Repository URL")]
        public string RepositoryURL { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Tkt Owner")]
        public string AuthorId { get; set; }

        public string Title { get; set; }
        [Required]
        [Display(Name = "Issue Type")]
        public int IssueTypeId { get; set; }

        [Display(Name = "Response Leader")]
        public string LeaderId { get; set; }

        [Required]
        [Display(Name = "Priority")]
        public int PriorityId { get; set; }

        [Required]
        [Display(Name = "Project")]
        public int ProjectId { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Tkt Stage")]
        public int StageId { get; set; }
        


        public virtual IssueType IssueType { get; set; }
        public virtual ApplicationUser Leader { get; set; }
        public virtual Priority Priority { get; set; }
        public virtual Project Project { get; set; }
        public virtual Stage Stage { get; set; }
        public virtual ApplicationUser Author { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Log> Logs { get; set; }
        public virtual ICollection<Notice> Notices { get; set; }

    }
    
}