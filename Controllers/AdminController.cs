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
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager, ILogger<AdminController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
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
    _logger.LogInformation("ManageStudents POST received: Name={Name} Email={Email}", Name, Email);
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

        // POST: Admin/AddStudent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(string Name, string Email, string GradeLevel)
        {
            _logger.LogInformation("AddStudent POST received: Name={Name} Email={Email}", Name, Email);

            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email))
            {
                ModelState.AddModelError("", "Name and Email are required.");
            }
            else
            {
                var user = new IdentityUser
                {
                    UserName = Email,
                    Email = Email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, "Student123!");
                if (result.Succeeded)
                {
                    _logger.LogInformation("Identity user created for {Email}", Email);
                    await _userManager.AddToRoleAsync(user, "Student");

                    var student = new Student
                    {
                        Name = Name,
                        Email = Email,
                        GradeLevel = GradeLevel,
                        IdentityUserId = user.Id
                    };

                    try
                    {
                        _context.Students.Add(student);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Student record created for {Email}", Email);
                        // On success redirect back to list
                        return RedirectToAction(nameof(ManageStudents));
                    }
                    catch (System.Exception ex)
                    {
                        _logger.LogError(ex, "Failed to save student record for {Email}", Email);
                        ModelState.AddModelError("", "Failed to save student record.");
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to create Identity user for {Email}: {Errors}", Email, string.Join(";", result.Errors.Select(e => e.Description)));
                    foreach (var e in result.Errors)
                        ModelState.AddModelError("", e.Description);
                }
            }

            // If we reach here there were validation errors or save errors — show them on the ManageStudents view
            var students = _context.Students.Include(s => s.IdentityUser).ToList();
            return View("ManageStudents", students);
        }

        // GET: Admin/EditStudent/{id}
        public IActionResult EditStudent(int id)
        {
            var student = _context.Students.FirstOrDefault(s => s.Id == id);
            if (student == null) return NotFound();
            return View(student);
        }

        // POST: Admin/EditStudent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudent(Student model)
        {
            if (!ModelState.IsValid) return View(model);

            var student = _context.Students.FirstOrDefault(s => s.Id == model.Id);
            if (student == null) return NotFound();

            // Update Student fields
            student.Name = model.Name;
            student.GradeLevel = model.GradeLevel;

            // If email changed, update Identity user and student email
            if (!string.Equals(student.Email, model.Email, System.StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(student.IdentityUserId))
                {
                    var identityUser = await _userManager.FindByIdAsync(student.IdentityUserId);
                    if (identityUser != null)
                    {
                        identityUser.Email = model.Email;
                        identityUser.UserName = model.Email;
                        await _userManager.UpdateAsync(identityUser);
                    }
                }

                student.Email = model.Email;
            }

            _context.Students.Update(student);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageStudents));
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
        // GET: Admin/ManageClasses
        [HttpGet]
        public IActionResult ManageClasses()
        {
            var classes = _context.Classes.ToList();
            return View(classes);
        }

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

