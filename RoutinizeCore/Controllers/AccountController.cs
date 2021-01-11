using MongoLibrary.Interfaces;
using RoutinizeCore.Services;
using RoutinizeCore.Services.Interfaces;

namespace RoutinizeCore.Controllers {

    public sealed class AccountController : CacheServiceBase {

        private readonly IRoutinizeCoreLogService _coreLogService;
        private readonly IAccountService _accountService;
        private readonly IUserService _userService;
        
    }
}