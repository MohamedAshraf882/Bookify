using AutoMapper;

namespace Bookify.Core.Mapping
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            //category mapping
            CreateMap<Category, CategoryViewModel>();
           CreateMap< CategoryFormViewModel, Category>().ReverseMap();
        }


    }
}
