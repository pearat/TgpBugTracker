using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace TgpBugTracker.Helpers
{
    public static class Extensions
    {
        public static string GetFullName(this IIdentity user)
        {
            var claimsIdentity = (ClaimsIdentity)user;
            var FullName = claimsIdentity.Claims.FirstOrDefault(c => c.Type == "FullName");
            if (FullName != null)
                return FullName.Value;
            else
                return null;
        }

        public static bool HasFullName(this IIdentity user)
        {
            var cUser = (ClaimsIdentity)user;
            var hasFN = cUser.Claims.FirstOrDefault(c => c.Type == "FullName");
            return (hasFN !=null && !string.IsNullOrWhiteSpace(hasFN.Value));
 
        }
    }
}