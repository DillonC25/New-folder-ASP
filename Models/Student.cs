using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace SeacoastUniversity.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? GradeLevel { get; set; }

        // Link to ASP.NET Identity user
        public string? IdentityUserId { get; set; }
        public IdentityUser? IdentityUser { get; set; }

        public List<ClassEnrollment> Enrollments { get; set; } = new();
    }
}
