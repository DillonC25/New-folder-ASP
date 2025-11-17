using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SeacoastUniversity.Models;

namespace SeacoastUniversity.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure DB is migrated (SQLite safe)
            await context.Database.MigrateAsync();

            // Seed roles
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("Student"))
                await roleManager.CreateAsync(new IdentityRole("Student"));

            // Seed admin user
            var adminEmail = "admin@seacoast.edu";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(adminUser, "Admin123!");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Seed courses
            if (!context.Classes.Any())
            {
                context.Classes.AddRange(
                    new Class { CourseName = "Intro to Programming", Instructor = "Dr. Smith" },
                    new Class { CourseName = "Data Structures", Instructor = "Prof. Lee" },
                    new Class { CourseName = "Web Development", Instructor = "Dr. Johnson" }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
