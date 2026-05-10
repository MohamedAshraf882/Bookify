using Bookify.Data;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            //TODO:use viewmodel

            var categories = _context.Categories.ToList();
            return View(categories);

        }

    }
}
