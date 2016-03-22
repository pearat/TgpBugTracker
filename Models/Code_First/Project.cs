using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TgpBugTracker.Models
{
    public class Project
    {
        public Project()
        {
            this.Tickets = new HashSet<Ticket>();
            this.Users = new HashSet<ApplicationUser>();
        }
        [Required]
        public int Id { get; set; }

        public DateTimeOffset Deadline { get; set; }
        public string Description { get; set; }
        public bool IsGuest { get; set; }
        [Required]
        public string Name { get; set; }
        public DateTimeOffset Started { get; set; }
        public string Version { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; }
        public virtual ICollection<ApplicationUser> Users { get; set; }
    }

    public class ProjectIdName
    {
        public int Id { get; set; }
        [Display(Name = "Project Name")]
        public string Name { get; set; }
    }
}