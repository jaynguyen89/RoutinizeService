using Microsoft.AspNetCore.Mvc;
using NotifierLibrary.Interfaces;
using RoutinizeCore.Attributes;
using RoutinizeCore.Services.Interfaces;

namespace RoutinizeCore.Controllers {

    [ApiController]
    [RoutinizeActionFilter]
    [Route("cooperation")]
    public sealed class CooperationController : AppController {

        private readonly ICooperationService _cooperationService;

        public CooperationController(
            ICooperationService cooperationService,
            IUserNotifier userNotifier
        ) : base(userNotifier) {
            _cooperationService = cooperationService;
        }
        
        
    }
}