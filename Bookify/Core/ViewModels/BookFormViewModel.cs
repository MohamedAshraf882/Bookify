using Bookify.Consts;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookify.Core.ViewModels
{
    public class BookFormViewModel
    {
        public int Id { get; set; }

        ////////////////////////////////////////// 
        
        [Remote("IsAllowed", null!, AdditionalFields = "Id,AuthorId", ErrorMessage = Errors.BookDuplicated)]
        [MaxLength(500, ErrorMessage = Errors.MaxLength)]
        public string Title { get; set; } = null!;


        [Required]
        [Remote("IsAllowed", null!, AdditionalFields = "Id,Title", ErrorMessage = Errors.BookDuplicated)]
        [Display(Name = "Author")]
        public int AuthorId { get; set; }


        /// to fill the dropdown list of authors in the form
        public IEnumerable<SelectListItem>? Authors { get; set; }


        [MaxLength(200, ErrorMessage = Errors.MaxLength)]
        public string Publisher { get; set; } = null!;


        
        [Display(Name = "Publishing Date")]
        [AssertThat("PublishingDate <= Now()",ErrorMessage =Errors.allowedDate)]
        public DateTime PublishingDate { get; set; } = DateTime.Now;


        public IFormFile? Image { get; set; }

        public string? ImageUrl { get; set; }

        [MaxLength(100, ErrorMessage = Errors.MaxLength)]
        public string Hall { get; set; } = null!;

        [Display(Name = "Is Available For Rental ?")]
        public bool IsAvailableForRental { get; set; }

        
        [MaxLength(1000, ErrorMessage = Errors.MaxLength)]
        [Required]
        public string Descreption { get; set; } = null!;

        [Required]
        [Display(Name ="Categories")]
        public IList<int>SelectedCategories { get; set; } = new List<int>();
        
        public IEnumerable<SelectListItem>? Categories { get; set; }

    }
}
