using Bookify.Consts;

namespace Bookify.Core.ViewModels
{
    public class AuthorFormViewModel
    {

        public int ID { get; set; }
        [Remote("IsAllowed", "Author", AdditionalFields = "ID", ErrorMessage = Errors.Duplicated)]
        [MaxLength(100, ErrorMessage = Errors.MaxLength), Display(Name = "Author")]
        public string Name { get; set; } = null!;
    }
}
