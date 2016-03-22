using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace TgpBugTracker.Models
{


    public class UserRolesVM : IComparable<UserRolesVM>
    {
        [DataType(DataType.Text)]
        [Display(Name = "Staff Member")]
        public string DisplayName { get; set; }
        public bool IsGuest { get; set; }
        public string[] Roles { get; set; }
        public MultiSelectList Selected { get; set; }
        // public string Selected { get; set; }
        public string AuthLevel { get; set; }
        public int RoleRank { get; set; }
        public string UserId { get; set; }

        public int CompareTo(UserRolesVM d)
        {
            return DisplayName.CompareTo((string)d.DisplayName);
        }
    }

    public class ProjectUsersVM : IComparable<ProjectUsersVM>
    {
        public string ProjectName { get; set; }
        public string[] Usrs { get; set; }
        public string[] Admins { get; set; }
        public string[] PjtMgrs { get; set; }
        public string[] Developers { get; set; }
        public string[] Submitters { get; set; }
        public string[] Unassigned { get; set; }
        public int TeamCount { get; set; }
        public int ProjectId { get; set; }
        public MultiSelectList Selected { get; set; }

        public int CompareTo(ProjectUsersVM p)
        {
            return ProjectName.CompareTo((string)p.ProjectName);
        }
    }


    public class Progress   // for DashboardVM
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Version { get; set; }
        public string Manager { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "Tkt Date")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yy}")]
        public DateTime Deadline { get; set; }
        public string Phase { get; set; }
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:#%}")]
        public double ProgressPct { get; set; }
    }

    public class DashboardVM
    {
        public DashboardVM()
        {
            this.Cmts = new List<Comment>();
            this.Ntcs = new List<NoticeVM>();
            this.Pjts = new List<Progress>();
            this.Tkts = new List<Ticket>();
            this.Team = new List<ApplicationUser>();
        }
        public int AttachmentCount { get; set; }
        public List<Comment> Cmts { get; set; }
        public List<NoticeVM> Ntcs { get; set; }
        public List<Progress> Pjts { get; set; }
        public List<Ticket> Tkts { get; set; }
        public List<ApplicationUser> Team { get; set; }
    }

    public class NoticeVM
    {

        [DataType(DataType.Date)]
        [Display(Name = "Due Date")]
        [DisplayFormat(DataFormatString = "{0:MM/dd}")]
        public DateTimeOffset Date { get; set; }

        public string ProjectName { get; set; }

        public string Title { get; set; }
    }

    public class UtilitiesVM
    {
        public UtilitiesVM()
        {
            this.Issues     = new List<IssueType>();
            this.Phases     = new List<Phase>();
            this.Priorities = new List<Priority>();
            this.Stages     = new List<Stage>();
        }
        public List<IssueType>  Issues { get; set; }
        public List<Phase>      Phases { get; set; }
        public List<Priority>   Priorities { get; set; }
        public List<Stage>      Stages { get; set; }
    }
}

