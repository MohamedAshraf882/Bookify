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
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Linq.Dynamic.Core;

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

        
        [HttpPost]
        public IActionResult GetBooks()
        {
            var skip = int.Parse(Request.Form["start"]);
            var pageSize = int.Parse(Request.Form["length"]);

            var searchvalue = Request.Form["search[value]"];

            var sortcolumnindex = Request.Form["order[0][column]"];
            var sortcolumn = Request.Form[$"columns[{sortcolumnindex}][name]"];
            var sortcolumndirection = Request.Form["order[0][dir]"];

            IQueryable<Book> books = _context.Books
                .Include(b=>b.Author)
                .Include(b=>b.Categories)
                .ThenInclude(c=>c.Category);

            if (!string.IsNullOrEmpty(searchvalue))
            {
                books = books.Where(b => b.Title.Contains(searchvalue)||b.Author!.Name.Contains(searchvalue));
            }

           books= books.OrderBy($"{sortcolumn} {sortcolumndirection}");
            
            //.Skip(0).Take(10).ToList();
            var data = books.Skip(skip).Take(pageSize).ToList();

            var mappedData= _mapper.Map<IEnumerable<BookViewModel>>(data);

            var recordstotal = books.Count();

            var jsonData = new { 
                recordsFiltered=recordstotal,
                recordsTotal=recordstotal,
                data=mappedData};

            return Ok(jsonData);

        }



        public IActionResult Details(int id)
        {
            var book=_context.Books
                .Include(b=>b.Author)
                .Include(b=>b.Copies)
                .Include(b=>b.Categories)
                .ThenInclude(c=>c.Category)
                .SingleOrDefault(b=>b.Id==id);
            if (book is null)
            {
                return NotFound();
            }
            var viewmodel= _mapper.Map<BookViewModel>(book);
            return View(viewmodel);

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

                var path = Path.Combine($"{_webHostEnvironment.WebRootPath}/Images/Books", ImageName);
                var thumbpath = Path.Combine($"{_webHostEnvironment.WebRootPath}/Images/Books/Thumb", ImageName);

                using var stream = System.IO.File.Create(path);
                await model.Image.CopyToAsync(stream);
                stream.Dispose();
                
                book.ImageUrl = $"/Images/Books/{ImageName}";
                book.ThumbnailImageUrl = $"/Images/Books/Thumb/{ImageName}";

                using var image=Image.Load(model.Image.OpenReadStream());
                var ratio = (float)image.Width / 200;
                var height=image.Height / ratio;
                image.Mutate(i => i.Resize(width:200,height:(int)height));
                image.Save(thumbpath);

                //using var stream = model.Image.OpenReadStream();
                //var imageparams = new ImageUploadParams
                //{
                //    File = new FileDescription(ImageName, stream),
                //    UseFilename=true,

                //};
                //var result = await _cloudinary.UploadAsync(imageparams);
                //book.ImageUrl = result.SecureUrl.ToString();
                //book.ThumbnailImageUrl=GetThumbnailImageUrl(book.ImageUrl);
                //book.ImagePublicId= result.PublicId;

            }


            foreach (var category in model.SelectedCategories)
                book.Categories.Add(new BookCategory { CategoryId = category });

            _context.Books.Add(book);
            _context.SaveChanges();
            return RedirectToAction(nameof(Details), new {id=book.Id});


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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BookFormViewModel model)
        {
            //string imagepublicid = null;
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
                    var oldimagepath = $"{_webHostEnvironment.WebRootPath}{book.ImageUrl}";
                    var oldthumbimagepath = $"{_webHostEnvironment.WebRootPath}{book.ThumbnailImageUrl}";
                    if (System.IO.File.Exists(oldimagepath))
                    {
                        System.IO.File.Delete(oldimagepath);
                    }
                    if (System.IO.File.Exists(oldthumbimagepath))
                    {

                        System.IO.File.Delete(oldthumbimagepath);
                    }

                    //await _cloudinary.DeleteResourcesAsync(book.ImagePublicId);

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

                var path = Path.Combine($"{_webHostEnvironment.WebRootPath}/Images/Books", imageName);
                var thumbPath = Path.Combine($"{_webHostEnvironment.WebRootPath}/Images/Books/Thumb", imageName);

                using var stream = System.IO.File.Create(path);
                await model.Image.CopyToAsync(stream);
                stream.Dispose();

                model.ImageUrl = $"/Image/Books/{imageName}";
                model.ThumbnailImageUrl = $"/Images/Books/Thumb/{imageName}";

                using var image = Image.Load(model.Image.OpenReadStream());
                var ratio = (float)image.Width / 200;
                var height = image.Height / ratio;
                image.Mutate(i => i.Resize(width: 200, height: (int)height));
                image.Save(thumbPath);

                // using var stream = model.Image.OpenReadStream();
                // var Imagparme = new ImageUploadParams 
                // { 
                //     File=new FileDescription(imageName, stream),
                //     UseFilename = true
                // };
                // var result=await _cloudinary.UploadAsync(Imagparme);
                // model.ImageUrl = result.SecureUrl.ToString();
                //imagepublicid=result.PublicId;

            }
            else if (!string.IsNullOrEmpty(book.ImageUrl))
            {
                model.ImageUrl = book.ImageUrl;
                model.ThumbnailImageUrl = book.ThumbnailImageUrl;
            }


            book = _mapper.Map(model, book);
            book.LastUpdatedOn = DateTime.Now;

            //book.ThumbnailImageUrl = GetThumbnailImageUrl(book.ImageUrl!);

            //book.ImagePublicId = imagepublicid;
            foreach (var category in model.SelectedCategories)
            {
                book.Categories.Add(new BookCategory { CategoryId = category });
            }

            _context.SaveChanges();
            return RedirectToAction(nameof(Details), new { id = book.Id });
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            var book = _context.Books.Find(id);
            if(book is null)
            {
                return NotFound();
            }
            book.IsDeleted = !book.IsDeleted;
            book.LastUpdatedOn = DateTime.Now;
            _context.SaveChanges();
            return Ok();
        }
    }
}
