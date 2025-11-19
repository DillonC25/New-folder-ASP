using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SeacoastUniversity.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        public string GradeLevel { get; set; } = "";

        public string? IdentityUserId { get; set; }
        public IdentityUser? IdentityUser { get; set; }

        // ADD THIS ↓↓↓
        public List<Enrollment>? Enrollments { get; set; }
    }
}
