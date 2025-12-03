using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SeacoastUniversity.Data;
using SeacoastUniversity.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// ======================
// APPLY MIGRATIONS + SEED DATA
// ======================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();

    // Seed default classes
    if (!context.Classes.Any())
    {
        context.Classes.AddRange(
            new Class { CourseName = "Intro to Programming", Instructor = "Dr. Smith" },
            new Class { CourseName = "Data Structures", Instructor = "Prof. Lee" }
        );
        context.SaveChanges();
    }
}

// ======================
// SEED ROLES
// ======================
async Task EnsureRolesAsync(IHost app)
{
    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole("Admin"));

    if (!await roleManager.RoleExistsAsync("Student"))
        await roleManager.CreateAsync(new IdentityRole("Student"));
}

// ======================
// SEED ADMIN USER
// ======================
async Task EnsureAdminUserAsync(IHost app)
{
    using var scope = app.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

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
}

async Task EnsureStudentUserAsync(IHost app)
{
    using var scope = app.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    var studentEmail = "student@seacoast.edu";
    var studentUser = await userManager.FindByEmailAsync(studentEmail);

    if (studentUser == null)
    {
        studentUser = new IdentityUser
        {
            UserName = studentEmail,
            Email = studentEmail,
            EmailConfirmed = true
        };

        await userManager.CreateAsync(studentUser, "Student123!");
        await userManager.AddToRoleAsync(studentUser, "Student");

        // Optional: Add Student record in your Students table
        if (!context.Students.Any(s => s.IdentityUserId == studentUser.Id))
        {
            var student = new Student
            {
                Name = "Default Student",
                GradeLevel = "Freshman",
                IdentityUserId = studentUser.Id
            };

            context.Students.Add(student);
            await context.SaveChangesAsync();
        }
    }
}

// ===== RUN SEEDING =====
await EnsureRolesAsync(app);      // ✅ Ensure all roles exist first
await EnsureAdminUserAsync(app);  // ✅ Then create admin
await EnsureStudentUserAsync(app); 
app.Run();
