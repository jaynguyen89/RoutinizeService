using MongoLibrary.Interfaces;
using RoutinizeCore.DbContexts;
using RoutinizeCore.Services.Interfaces;

namespace RoutinizeCore.Services.DatabaseServices {

    public sealed class AccountService : CacheServiceBase, IAccountService {

        private readonly IRoutinizeCoreLogService _coreLogService;
        private readonly RoutinizeDbContext _routinizeDbContext;

        public AccountService(
            IRoutinizeCoreLogService coreLogService,
            RoutinizeDbContext routinizeDbContext
        ) {
            _coreLogService = coreLogService;
            _routinizeDbContext = routinizeDbContext;
        }
    }
}