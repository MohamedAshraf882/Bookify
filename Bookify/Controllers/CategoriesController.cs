namespace Bookify.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public CategoriesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Index()
        {
            //TODO:use viewmodel

            var categories = _context.Categories
            //.Select(c => new CategoryViewModel
            //{
            //    Id = c.Id,
            //    Name = c.Name,
            //    IsDeleted = c.IsDeleted,
            //    CreatedOn = c.CreatedOn,
            //    LastUpdatedOn = c.LastUpdatedOn

            //})
            .AsNoTracking()
            .ToList();
            //use auto mapper
            var viewModel = _mapper.Map<IEnumerable<CategoryViewModel>>(categories);
            return View(viewModel);

        }

        [HttpGet]
        [AjaxOnly]
        public IActionResult Create()
        {

            return PartialView("_Form");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
                //return View("_Form", model);
            }

            //var category = new Category();
            //category.Name = model.Name;
            var category=_mapper.Map<Category>(model);

            _context.Categories.Add(category);
            _context.SaveChanges();

            //var categoryViewModel = new CategoryViewModel
            //{
            //    Id = category.Id,
            //    Name = category.Name,
            //    IsDeleted = category.IsDeleted,
            //    CreatedOn = category.CreatedOn,
            //    LastUpdatedOn = category.LastUpdatedOn
            //};
            var categoryViewModel = _mapper.Map<CategoryViewModel>(category);

            return PartialView("_CategoryRow", categoryViewModel);
            //return RedirectToAction(nameof(Index));

        }

        [HttpGet]
        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var category = _context.Categories.Find(id);
            if (category is null)
                return NotFound();
            //var model = new CategoryFormViewModel
            //{
            //    ID = id,
            //    Name = category.Name,
            //};
            var model = _mapper.Map<CategoryFormViewModel>(category);

            return PartialView("_Form", model);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
                // return View("_Form", model);
            }
            var category = _context.Categories.Find(model.ID);
            if(category is null)
            {
                return NotFound();
            }
            //category.Name = model.Name;
            category = _mapper.Map(model, category);
            category.LastUpdatedOn = DateTime.Now;

            _context.SaveChanges();

            //var categoryViewModel = new CategoryViewModel
            //{
            //    Id = category.Id,
            //    Name = category.Name,
            //    IsDeleted = category.IsDeleted,
            //    CreatedOn = category.CreatedOn,
            //    LastUpdatedOn = category.LastUpdatedOn
            //};
            var categoryViewModel = _mapper.Map<CategoryViewModel>(category);

            return PartialView("_CategoryRow", categoryViewModel);
            // return PartialView("_CategoryRow", category);
            //return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            var category = _context.Categories.Find(id);

            if (category is null)
                return NotFound();

            category.IsDeleted = !category.IsDeleted;
            category.LastUpdatedOn = DateTime.Now;

            _context.SaveChanges();

            return Ok(category.LastUpdatedOn.ToString());
        }
        public IActionResult AllowItem(CategoryFormViewModel model)
        {
            var category = _context.Categories.SingleOrDefault(c => c.Name == model.Name);
            var isAllowed = category is null || category.Id.Equals(model.ID);
            return Json(isAllowed);
        }


    }
}
