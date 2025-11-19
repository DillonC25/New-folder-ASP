using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeacoastUniversity.Data;
using SeacoastUniversity.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SeacoastUniversity.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =========================
        // DASHBOARD
        // =========================
        public IActionResult Dashboard()
        {
            var vm = new AdminDashboardVM
            {
                TotalStudents = _context.Students.Count(),
                TotalClasses = _context.Classes.Count(),
                NewestStudents = _context.Students.OrderByDescending(s => s.Id).Take(5).ToList()
            };
            return View(vm);
        }

        // =========================
        // STUDENTS
        // =========================
        // GET: Admin/ManageStudents
[HttpGet]
public IActionResult ManageStudents()
{
    // Load all students including their IdentityUser info (email)
    var students = _context.Students
                           .Include(s => s.IdentityUser)
                           .ToList();
    return View(students);
}

// POST: Admin/ManageStudents
[HttpPost]
public async Task<IActionResult> ManageStudents(string Name, string Email, string GradeLevel)
{
    if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email))
    {
        ModelState.AddModelError("", "Name and Email are required.");
    }
    else
    {
        // Create Identity user
        var user = new IdentityUser
        {
            UserName = Email,
            Email = Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, "Student123!");
        if (result.Succeeded)
        {
            // Assign Student role
            await _userManager.AddToRoleAsync(user, "Student");

            // Create Student record
            var student = new Student
            {
                Name = Name,
                GradeLevel = GradeLevel,
                IdentityUserId = user.Id
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();
        }
        else
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError("", e.Description);
        }
    }

    // Reload students
    var students = _context.Students.Include(s => s.IdentityUser).ToList();
    return View(students);
}


// DELETE student
public async Task<IActionResult> DeleteStudent(int id)
{
    var student = await _context.Students.FindAsync(id);
    if (student != null)
    {
        // Delete linked Identity user
        if (!string.IsNullOrEmpty(student.IdentityUserId))
        {
            var identityUser = await _userManager.FindByIdAsync(student.IdentityUserId);
            if (identityUser != null)
                await _userManager.DeleteAsync(identityUser);
        }

        _context.Students.Remove(student);
        await _context.SaveChangesAsync();
    }

    return RedirectToAction(nameof(ManageStudents));
}


        // =========================
        // CLASSES
        // =========================
        [HttpPost]
        public IActionResult ManageClasses(Class model)
        {
            if (ModelState.IsValid)
            {
                _context.Classes.Add(model);
                _context.SaveChanges();
                
            }

        // Reload the table with all classes
        var classes = _context.Classes.ToList();
        return View(classes);
    }


        [HttpPost]
        public IActionResult AddClass(Class model)
        {
            if (ModelState.IsValid)
            {
                _context.Classes.Add(model);
                _context.SaveChanges();
                return RedirectToAction(nameof(ManageClasses));
            }

            var classes = _context.Classes.ToList();
            return View("ManageClasses", classes);
        }

        public IActionResult EditClass(int id)
        {
            var classObj = _context.Classes.FirstOrDefault(c => c.Id == id);
            if (classObj == null) return NotFound();
            return View(classObj);
        }

        [HttpPost]
        public IActionResult EditClass(Class model)
        {
            if (!ModelState.IsValid) return View(model);

            _context.Classes.Update(model);
            _context.SaveChanges();
            return RedirectToAction(nameof(ManageClasses));
        }

        public IActionResult DeleteClass(int id)
        {
            var classObj = _context.Classes.FirstOrDefault(c => c.Id == id);
            if (classObj == null) return NotFound();

            _context.Classes.Remove(classObj);
            _context.SaveChanges();
            return RedirectToAction(nameof(ManageClasses));
        }
    }
}

