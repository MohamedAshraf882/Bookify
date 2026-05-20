using Microsoft.AspNetCore.Mvc;

namespace Bookify.Core.ViewModels
{
    public class CategoryFormViewModel
    {

        public int ID { get; set; }

        [Remote( "AllowItem", null,AdditionalFields ="ID", ErrorMessage = "This name is already exist.")]
        [MaxLength(100, ErrorMessage = "Max length Cannot be more 100 chr.")]
        public string Name { get; set; }
    }
}
