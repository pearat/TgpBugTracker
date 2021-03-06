﻿using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.Security.Principal;
using System.Linq;

namespace TgpBugTracker.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            this.Projects = new HashSet<Project>();
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string DisplayName { get; set; }

        public string FullName { get{ return FirstName + " " + LastName; } }

        public string Greeting { get { return "Hello, "+FirstName + "!"; } }

        public int RoleRank { get; set; }

        public bool IsGuest { get; set; }
        public bool Active { get; set; }

        public virtual ICollection<Project> Projects { get; set; }

        // public string Team { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            userIdentity.AddClaim(new Claim("FullName", FullName.ToString()));
            userIdentity.AddClaim(new Claim("Greeting", Greeting.ToString()));
            return userIdentity;
        }


        public static string GetFullName(IIdentity user)
        {
            var claimsIdentity = (ClaimsIdentity)user;
            var FullName = claimsIdentity.Claims.FirstOrDefault(c => c.Type == "FullName");
            if (FullName != null)
                return FullName.Value;
            else
                return null;
        }


    }

    public class ApplicationUserVM
    {

        public string Id { get; set; }
        public string DisplayName { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<IssueType> IssueTypes { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Notice> Notices { get; set; }
        public DbSet<Priority> Priorities { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Stage> Stages { get; set; }
        public DbSet<Phase> Phases { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        // public DbSet<UserProject> UserProjects { get; set; }
    }
}