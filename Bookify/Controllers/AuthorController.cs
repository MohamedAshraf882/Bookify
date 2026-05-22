using Microsoft.AspNetCore.Mvc;

namespace Bookify.Controllers
{
    public class AuthorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public AuthorController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var authors = _context.Authors.AsNoTracking().ToList();
            var authorsviewmodel = _mapper.Map<IEnumerable<AuthorViewModel>>(authors);
            return View(authorsviewmodel);
        }

        [HttpGet]
        [AjaxOnly]
        public IActionResult Create()
        {
            return PartialView("_Form");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AuthorFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var author = _mapper.Map<Author>(model);
            _context.Authors.Add(author);
            _context.SaveChanges();
            var authorviewmodel = _mapper.Map<AuthorViewModel>(author);
            return PartialView("_AuthorRow", authorviewmodel);
        }

        public IActionResult IsAllowed(AuthorFormViewModel model)
        {
            var author = _context.Authors.SingleOrDefault(a => a.Name == model.Name);
            var IsAllowed = author is null || author.Id.Equals(model.ID);
            return Json(IsAllowed);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            var author = _context.Authors.Find(id);
            if (author is null)
                return NotFound();
            author.IsDeleted = !author.IsDeleted;
            author.LastUpdatedOn = DateTime.Now;
            _context.SaveChanges();
            return Ok(author.LastUpdatedOn.ToString());
        }

        [HttpGet]
        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var author = _context.Authors.Find(id);
            if (author is null)
                return NotFound();
            var model = _mapper.Map<AuthorFormViewModel>(author);
            return PartialView("_Form", model);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(AuthorFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var author = _context.Authors.Find(model.ID);
            if (author is null)
                return NotFound();
            author = _mapper.Map(model, author);
            author.LastUpdatedOn = DateTime.Now;
            _context.SaveChanges();
            var authorviewmodel = _mapper.Map<AuthorViewModel>(author);
            return PartialView("_AuthorRow", authorviewmodel);
        }
    }
}
