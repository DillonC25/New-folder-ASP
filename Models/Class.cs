using System.ComponentModel.DataAnnotations;

namespace SeacoastUniversity.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        public string CourseName { get; set; }

        [Required]
        public string Instructor { get; set; }
    }
}
