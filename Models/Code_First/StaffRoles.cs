using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TgpBugTracker.Models
{
    public class StaffRoles : IComparable<StaffRoles>
    {

        public string DisplayName { get; set; }
        public bool IsGuest { get; set; }
        public string[] Roles { get; set; }
        public MultiSelectList Selected { get; set; }
        public string UserId { get; set; }

        public int CompareTo(StaffRoles d)
        {
            return DisplayName.CompareTo((string)d.DisplayName);
        }
    }

}