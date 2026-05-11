namespace Bookify.Core.ViewModels
{
    public class CategoryFormViewModel
    {

        public int ID { get; set; }
        [MaxLength(100, ErrorMessage = "Max length Cannot be more 100 chr.")]
        public string Name { get; set; }
    }
}
