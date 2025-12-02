using Microsoft.AspNetCore.Mvc;
using FancySignup.Models;

namespace FancySignup.Controllers
{
    public class WeekController : Controller
    {
        private readonly string _filePath = "Data/week.txt";

        public IActionResult Index()
        {
            var days = new List<WeekDay>();

            // Ensure file exists
            if (!System.IO.File.Exists(_filePath))
            {
                System.IO.File.WriteAllText(_filePath, "");
            }

            var lines = System.IO.File.ReadAllLines(_filePath);

            // If no notes exist yet, generate empty structure
            if (lines.Length == 0)
            {
                var today = DateTime.Today;

                for (int i = 0; i < 7; i++)
                {
                    var date = today.AddDays(i);

                    days.Add(new WeekDay
                    {
                        DayName = date.ToString("dddd"),
                        Date = date.ToString("MM/dd"),
                        Notes = ""
                    });
                }
            }
            else
            {
                foreach (var line in lines)
                {
                    var parts = line.Split("|");

                    days.Add(new WeekDay
                    {
                        DayName = parts[0],
                        Date = parts[1],
                        Notes = parts[2]
                    });
                }
            }

            return View(days);
        }

        [HttpPost]
        public IActionResult Save(List<WeekDay> days)
        {
            var lines = days.Select(d => $"{d.DayName}|{d.Date}|{d.Notes}").ToList();
            System.IO.File.WriteAllLines(_filePath, lines);

            return RedirectToAction("Index");
        }
    }
}
