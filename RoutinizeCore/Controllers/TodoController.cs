using Microsoft.AspNetCore.Mvc;
using RoutinizeCore.Services.Interfaces;

namespace RoutinizeCore.Controllers {

    [ApiController]
    [Route("todo")]
    public sealed class TodoController {

        private readonly IUserService _userService;

        public TodoController(
            IUserService userService
        ) {
            _userService = userService;
        }

        
    }
}