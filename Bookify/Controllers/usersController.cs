using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Controllers
{
    [Authorize(Roles =AppRoles.Admin)]
    public class usersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        public usersController(UserManager<ApplicationUser> usermanager, IMapper mapper)
        {
            _userManager = usermanager;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var users= await _userManager.Users.ToListAsync();
            var viewmodel = _mapper.Map<IEnumerable<UserViewmodel>>(users);
            return View(viewmodel);
        }
    }
}
