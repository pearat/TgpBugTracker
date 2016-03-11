using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
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
        //public MultiSelectList Selected { get; set; }
        public string Selected { get; set; }
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
}