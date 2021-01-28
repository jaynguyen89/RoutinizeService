using Microsoft.AspNetCore.Mvc;
using RoutinizeCore.Services.Interfaces;

namespace RoutinizeCore.Controllers {

    public sealed class UserController : ControllerBase {

        private readonly IUserService _userService;
        private readonly IAddressService _addressService;

        public UserController(
            IUserService userService,
            IAddressService addressService
        ) {
            _userService = userService;
            _addressService = addressService;
        }
        
        
    }
}