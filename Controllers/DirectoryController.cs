using Microsoft.AspNetCore.Mvc;
using FancySignup.Models;

namespace FancySignup.Controllers
{
    public class DirectoryController : Controller
    {
        private readonly string _filePath = "Data/directory.txt";

        public IActionResult Index(string search = "")
        {
            var people = LoadPeople();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                people = people
                    .Where(p =>
                        p.Name.ToLower().Contains(search) ||
                        p.Email.ToLower().Contains(search) ||
                        p.Department.ToLower().Contains(search) ||
                        p.Role.ToLower().Contains(search))
                    .ToList();
            }

            ViewBag.Search = search;

            return View(people);
        }

        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Add(DirectoryPerson person)
        {
            var line = $"{person.Name}|{person.Email}|{person.Phone}|{person.Department}|{person.Role}";
            System.IO.File.AppendAllLines(_filePath, new[] { line });

            return RedirectToAction("Index");
        }

        private List<DirectoryPerson> LoadPeople()
        {
            if (!System.IO.File.Exists(_filePath))
                return new List<DirectoryPerson>();

            return System.IO.File
                .ReadAllLines(_filePath)
                .Select(line =>
                {
                    var parts = line.Split("|");
                    return new DirectoryPerson
                    {
                        Name = parts[0],
                        Email = parts[1],
                        Phone = parts[2],
                        Department = parts[3],
                        Role = parts[4]
                    };
                }).ToList();
        }
    }
}
