using Bookify.Consts;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Core.ViewModels
{
    public class CategoryFormViewModel
    {

        public int ID { get; set; }

        [Remote("AllowItem", null!, AdditionalFields = "ID", ErrorMessage = Errors.Duplicated)]
        [MaxLength(100, ErrorMessage = Errors.MaxLength), Display(Name = "Category")]
        public string Name { get; set; } = null!;
    }
}
