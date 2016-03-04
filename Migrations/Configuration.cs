namespace TgpBugTracker.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<TgpBugTracker.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        // protected override void Seed(TgpBugTracker.Models.ApplicationDbContext context)
        //{
        //  This method will be called after migrating to the latest version.

        //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
        //  to avoid creating duplicate seed data. E.g.
        //
        //    context.People.AddOrUpdate(
        //      p => p.FullName,
        //      new Person { FullName = "Andrew Peters" },
        //      new Person { FullName = "Brice Lambson" },
        //      new Person { FullName = "Rowan Miller" }
        //    );
        //
        // }


        protected override void Seed(TgpBugTracker.Models.ApplicationDbContext context)
        {
            var roleManager = new RoleManager<IdentityRole>(
            new RoleStore<IdentityRole>(context));

            // For Administrator
            if (!context.Roles.Any(r => r.Name == "Administrator"))
            {
                roleManager.Create(new IdentityRole { Name = "Admin" });
            }
            var userManager = new UserManager<Models.ApplicationUser>(
                new UserStore<ApplicationUser>(context));

            if (!context.Users.Any(u => u.Email == "tpeara@gmail.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "tpeara@gmail.com",
                    Email = "tpeara@gmail.com",
                    DisplayName = "Andy Brown"
                }, "Password-1");
            }
            var userId = userManager.FindByEmail("tpeara@gmail.com").Id;
            userManager.AddToRole(userId, "Admin");

            // For Project Manager
            if (!context.Roles.Any(r => r.Name == "Project Manager"))
            {
                roleManager.Create(new IdentityRole { Name = "Project Manager" });
            }
            if (!context.Users.Any(u => u.Email == "tim@peara.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "tim@peara.com",
                    Email = "tim@peara.com",
                    DisplayName = "Paul Mann"
                }, "Password-1");
            }
            userId = userManager.FindByEmail("tim@peara.com").Id;
            userManager.AddToRole(userId, "Project Manager");

            // For Developer
            if (!context.Roles.Any(r => r.Name == "Developer"))
            {
                roleManager.Create(new IdentityRole { Name = "Developer" });
            }
            if (!context.Users.Any(u => u.Email == "tim@theionizer.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "tim@theionizer.com",
                    Email = "tim@theionizer.com",
                    DisplayName = "Dong Da-Xai"
                }, "Password-1");
            }
            userId = userManager.FindByEmail("tim@theionizer.com").Id;
            userManager.AddToRole(userId, "Developer");

            // For Submitter
            if (!context.Roles.Any(r => r.Name == "Submitter"))
            {
                roleManager.Create(new IdentityRole { Name = "Submitter" });
            }
            if (!context.Users.Any(u => u.Email == "timpeara@altenergyfin.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "timpeara@altenergyfin.com",
                    Email = "timpeara@altenergyfin.com",
                    DisplayName = "Saburo Saito"
                }, "Password-1");
            }
            userId = userManager.FindByEmail("timpeara@altenergyfin.com").Id;
            userManager.AddToRole(userId, "Submitter");

        }

    }
}
