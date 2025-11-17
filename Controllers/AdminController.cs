using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeacoastUniversity.Data;
using SeacoastUniversity.Models;

namespace SeacoastUniversity.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var students = await _context.Students.Include(s => s.Enrollments).ToListAsync();
            var classes = await _context.Classes.ToListAsync();
            ViewBag.Courses = classes;
            return View(students);
        }

        [HttpPost]
        public async Task<IActionResult> AddStudent(string name, string gradeLevel)
        {
            _context.Students.Add(new Student { Name = name, GradeLevel = gradeLevel });
            await _context.SaveChangesAsync();
            return RedirectToAction("Dashboard");
        }
        [HttpPost]
        public async Task<IActionResult> LinkStudentToUser(int studentId, string userEmail)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == studentId);

            if (user != null && student != null)
            {
                student.IdentityUserId = user.Id;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Dashboard");
        }
        public IActionResult ManageStudents()
        {
            var students = _context.Students.ToList();
            return View(students);
        }

        public IActionResult ManageClasses()
        {
            var courses = _context.Classes.ToList();
            return View(courses);
        }


    }
}
