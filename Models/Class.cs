namespace SeacoastUniversity.Models
{
    public class Class
    {
        public int Id { get; set; }

        public string CourseName { get; set; }
        public string Instructor { get; set; }

        public List<Enrollment> Enrollments { get; set; } = new();
    }
}
