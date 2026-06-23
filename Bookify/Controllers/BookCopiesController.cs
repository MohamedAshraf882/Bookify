using Microsoft.AspNetCore.Mvc;

namespace Bookify.Controllers
{
    public class BookCopiesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public BookCopiesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


        [AjaxOnly]
        public IActionResult Create(int bookid)
        {
            var Book = _context.Books.Find(bookid);
            if (Book is null)
            {
                return NotFound();
            }

            var viewmodel = new BookCopyFormViewModel 
            { 
                BookId = bookid,
               ShowRentalInput = Book.IsAvailableForRental,
            };

            return PartialView("Form", viewmodel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BookCopyFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var Book = _context.Books.Find(model.BookId);
            if (Book is null)
            {
                return NotFound();
            }

            var copy = new BookCopy
            {
                EditionNumber=model.EditionNumber,
                IsAvailableForRental= Book.IsAvailableForRental && model.IsAvailableForRental,

            };

            Book.Copies.Add(copy);
            _context.SaveChanges();
           
            var viewmodel= _mapper.Map< BookCopyViewModel>(copy);
            return PartialView("_BookCopyRow", viewmodel);

        }

        [AjaxOnly]
        public IActionResult Edit(int id) 
        {
            var copy=_context.BookCopies.Include(c=>c.Book).SingleOrDefault(c=>c.Id==id);
            if (copy is null)
                return NotFound();

            var viewmodel=_mapper.Map< BookCopyFormViewModel>(copy);
            viewmodel.ShowRentalInput=copy.Book!.IsAvailableForRental;
            return PartialView("Form", viewmodel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(BookCopyFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var copy=_context.BookCopies.Include(c=>c.Book).SingleOrDefault(c=>c.Id==model.Id);
            if(copy is null)
                return NotFound();

            copy.EditionNumber= model.EditionNumber;
            copy.IsAvailableForRental= copy.Book!.IsAvailableForRental&& model.IsAvailableForRental;
            copy.LastUpdatedOn=DateTime.Now;
            _context.SaveChanges();
            var viewmodel=_mapper.Map<BookCopyViewModel>(copy);
            return PartialView("_BookCopyRow", viewmodel);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStats(int id)
        {
            var copy = _context.BookCopies.Find(id);
            if (copy == null)
            {
                return NotFound();
            }
            copy.IsDeleted = !copy.IsDeleted;
            copy.LastUpdatedOn = DateTime.Now;
            _context.SaveChanges();
            return Ok();
        }
    }
}
