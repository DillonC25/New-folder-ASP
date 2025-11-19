namespace SeacoastUniversity.Models
{
    public class AdminDashboardVM
    {
        public int TotalStudents { get; set; }
        public int TotalClasses { get; set; }

        public List<Student> NewestStudents { get; set; } = new();
    }
}
