using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            //TODO:use viewmodel

            var categories = _context.Categories.AsNoTracking().ToList();
            return View(categories);

        }

        [HttpGet]
        public IActionResult Create()
        {

            return View("Form");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Form", model);
            }

            var category = new Category();
            category.Name = model.Name;

            _context.Categories.Add(category);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));

        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var category = _context.Categories.Find(id);
            if (category is null)
                return NotFound();
            var model = new CategoryFormViewModel
            {
                ID = id,
                Name = category.Name,
            };

            return View("Form", model);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Form", model);
            }
            var category = _context.Categories.Find(model.ID);
            if(category is null)
            {
                return NotFound();
            }
            category.Name = model.Name;
            category.LastUpdatedOn = DateTime.Now;
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

    }
}
