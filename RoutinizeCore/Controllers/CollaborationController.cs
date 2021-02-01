using Microsoft.AspNetCore.Mvc;
using RoutinizeCore.Services.Interfaces;

namespace RoutinizeCore.Controllers {

    [ApiController]
    [Route("collaboration")]
    public sealed class CollaborationController {

        private readonly ICollaborationService _collaborationService;
        private readonly IAccountService _accountService;
        private readonly IUserService _userService;

        public CollaborationController(
            ICollaborationService collaborationService,
            IAccountService accountService,
            IUserService userService
        ) {
            _collaborationService = collaborationService;
            _accountService = accountService;
            _userService = userService;
        }
        
        
    }
}