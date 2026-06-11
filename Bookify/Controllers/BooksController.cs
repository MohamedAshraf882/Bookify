using Azure.Messaging;
using Bookify.Consts;
using Bookify.Setting;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;

namespace Bookify.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly Cloudinary _cloudinary;
        private List<string> _AllowedExtensions = new List<string> { ".jpg", ".jpeg", ".png" };
        private int _maxAllowedSize = 21024 * 1024;
        public BooksController(ApplicationDbContext context, IMapper mapper,
            IWebHostEnvironment webHostEnvironment, IOptions<CloudinarySettings> cloudinary)
        {
            _context = context;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;

            Account account = new()
            {
                Cloud = cloudinary.Value.CloudName,
                ApiKey = cloudinary.Value.ApiKey,
                ApiSecret = cloudinary.Value.ApiSecret
            };

            _cloudinary = new Cloudinary(account);

        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {

            return View("Form", PopulateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookFormViewModel model)
        {

            if (!ModelState.IsValid)
            {

                return View("Form", PopulateViewModel(model));
            }

            var book = _mapper.Map<Book>(model);

            if (model.Image is not null)
            {

                var extension = Path.GetExtension(model.Image.FileName);
                if (!_AllowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(nameof(model.Image), Errors.NotAllowedExtensions);
                    return View("Form", PopulateViewModel(model));
                }
                if (model.Image.Length > _maxAllowedSize)
                {
                    ModelState.AddModelError(nameof(model.Image), Errors.MaxSize);
                    return View("Form", PopulateViewModel(model));
                }

                var ImageName = $"{Guid.NewGuid()}{extension}";

                //var path = Path.Combine($"{_webHostEnvironment.WebRootPath}/Images/Books", ImageName);

                //using var stream = System.IO.File.Create(path);
                //await model.Image.CopyToAsync(stream);
                //book.ImageUrl = ImageName;

                using var stream = model.Image.OpenReadStream();
                var imageparams = new ImageUploadParams
                {
                    File = new FileDescription(ImageName, stream),
                    UseFilename=true,

                };
                var result = await _cloudinary.UploadAsync(imageparams);
                book.ImageUrl = result.SecureUrl.ToString();
                book.ThumbnailImageUrl=GetThumbnailImageUrl(book.ImageUrl);
                book.ImagePublicId= result.PublicId;

            }


            foreach (var category in model.SelectedCategories)
                book.Categories.Add(new BookCategory { CategoryId = category });

            _context.Books.Add(book);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));


        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var book = _context.Books.Include(b => b.Categories).SingleOrDefault(b => b.Id == id);
            if (book is null)
            {

                return NotFound();
            }
            var model = _mapper.Map<BookFormViewModel>(book);
            var viewmodel = PopulateViewModel(model);

            viewmodel.SelectedCategories = book.Categories.Select(c => c.CategoryId).ToList();


            return View("Form", viewmodel);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(BookFormViewModel model)
        {
            string imagepublicid = null;
            if (!ModelState.IsValid)
            {
                return View("Form", PopulateViewModel(model));
            }

            var book = _context.Books.Include(b => b.Categories).SingleOrDefault(b => b.Id == model.Id);

            if (book is null)
            {
                return NotFound();
            }

            if (model.Image is not null)
            {
                if (!string.IsNullOrEmpty(book.ImageUrl))
                {
                    //var oldimagepath = Path.Combine($"{_webHostEnvironment.WebRootPath}/Images/Books", book.ImageUrl);
                    //if (System.IO.File.Exists(oldimagepath))
                    //{
                    //    System.IO.File.Delete(oldimagepath);
                    //}

                    await _cloudinary.DeleteResourcesAsync(book.ImagePublicId);

                }

                var extension = Path.GetExtension(model.Image.FileName);
                if (!_AllowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(nameof(model.Image), Errors.NotAllowedExtensions);
                    return View("Form", PopulateViewModel(model));
                }
                if (model.Image.Length > _maxAllowedSize)
                {
                    ModelState.AddModelError(nameof(model.Image), Errors.MaxSize);
                    return View("Form", PopulateViewModel(model));
                }

                var imageName = $"{Guid.NewGuid()}{extension}";

                //var path = Path.Combine($"{_webHostEnvironment.WebRootPath}/Images/Books", imageName);
                //using var stream = System.IO.File.Create(path);
                //await model.Image.CopyToAsync(stream);
                // model.ImageUrl = imageName;
                using var stream = model.Image.OpenReadStream();
                var Imagparme = new ImageUploadParams 
                { 
                    File=new FileDescription(imageName, stream),
                    UseFilename = true
                };
                var result=await _cloudinary.UploadAsync(Imagparme);
                model.ImageUrl = result.SecureUrl.ToString();
               imagepublicid=result.PublicId;

            }
            else if (!string.IsNullOrEmpty(book.ImageUrl))
                model.ImageUrl = book.ImageUrl;



            book = _mapper.Map(model, book);
            book.LastUpdatedOn = DateTime.Now;

            book.ThumbnailImageUrl = GetThumbnailImageUrl(book.ImageUrl!);

            book.ImagePublicId = imagepublicid;
            foreach (var category in model.SelectedCategories)
            {
                book.Categories.Add(new BookCategory { CategoryId = category });
            }

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        private BookFormViewModel PopulateViewModel(BookFormViewModel? model = null)
        {
            BookFormViewModel viewModel = model is null ? new BookFormViewModel() : model;
            var categories = _context.Categories.Where(c => !c.IsDeleted).OrderBy(a => a.Name).ToList();
            var authors = _context.Authors.Where(a => !a.IsDeleted).OrderBy(a => a.Name).ToList();

            viewModel.Authors = _mapper.Map<IEnumerable<SelectListItem>>(authors);
            viewModel.Categories = _mapper.Map<IEnumerable<SelectListItem>>(categories);
            return viewModel;
        }

        public IActionResult IsAllowed(BookFormViewModel model)
        {

            var book = _context.Books.SingleOrDefault(b => b.Title == model.Title && b.AuthorId == model.AuthorId);
            var isallowed = book is null || book.Id.Equals(model.Id);
            return Json(isallowed);
        }

        private string GetThumbnailImageUrl(string url)
        {
            //https://res.cloudinary.com/ashraf1/image/upload/v1781003763/ojpqz3zlpophozgnfs2l.jpg
            //https://res.cloudinary.com/ashraf1/image/upload/w_150,h_150,c_thumb/v1781003763/ojpqz3zlpophozgnfs2l.jpg
            var separator = "image/upload/";
            var urlparts = url.Split(separator);

            var thumbnailUrl = $"{urlparts[0]}{separator}c_thumb,w_200,g_face/{urlparts[1]}";
            return thumbnailUrl;
        }
    }
}
