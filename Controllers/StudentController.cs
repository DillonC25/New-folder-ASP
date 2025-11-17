using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeacoastUniversity.Data;
using SeacoastUniversity.Models;

namespace SeacoastUniversity.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public StudentController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var student = await _context.Students
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Class)
                .FirstOrDefaultAsync(s => s.IdentityUserId == user.Id);

            if (student == null)
            {
                // Redirect or show message if no linked student record
                ViewBag.Message = "No student record found for your account.";
                return View("NoStudent");
            }

            return View(student);
        }
    }
}
