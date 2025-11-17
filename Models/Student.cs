using Microsoft.AspNetCore.Identity;

namespace SeacoastUniversity.Models
{
    public class Student
    {
        public int Id { get; set; }

        // Link to ASP.NET Identity user
        public string IdentityUserId { get; set; }
        public IdentityUser IdentityUser { get; set; }

        // Student info
        public string Name { get; set; }
        public string GradeLevel { get; set; }

        // Navigation
        public List<Enrollment> Enrollments { get; set; } = new();
    }
}
