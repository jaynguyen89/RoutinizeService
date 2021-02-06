using Microsoft.AspNetCore.Mvc;
using RoutinizeCore.Attributes;
using RoutinizeCore.Services.Interfaces;

namespace RoutinizeCore.Controllers {

    [ApiController]
    [RoutinizeActionFilter]
    [Route("note")]
    public sealed class NoteController {
        
        private readonly INoteService _noteService;
        private readonly ICollaborationService _collaborationService;
        private readonly IUserService _userService;

        public NoteController(
            INoteService noteService,
            ICollaborationService collaborationService,
            IUserService userService
        ) {
            _noteService = noteService;
            _collaborationService = collaborationService;
            _userService = userService;
        }
    }
}