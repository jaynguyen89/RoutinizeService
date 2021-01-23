using System.Diagnostics;
using System.Threading.Tasks;
using HelperLibrary;
using HelperLibrary.Shared;
using Microsoft.EntityFrameworkCore;
using MongoLibrary.Interfaces;
using MongoLibrary.Models;
using RoutinizeCore.DbContexts;
using RoutinizeCore.Models;
using RoutinizeCore.Services.Interfaces;

namespace RoutinizeCore.Services.DatabaseServices {

    public sealed class UserService : CacheServiceBase, IUserService {
        
        private readonly IRoutinizeCoreLogService _coreLogService;
        private readonly RoutinizeDbContext _dbContext;

        public UserService(
            IRoutinizeCoreLogService coreLogService,
            RoutinizeDbContext dbContext
        ) {
            _coreLogService = coreLogService;
            _dbContext = dbContext;
        }

        public async Task<int?> InsertBlankUserOnAccountRegistration(int accountId) {
            try {
                var dbUser = new User { AccountId = accountId };
                await _dbContext.AddAsync(dbUser);

                var result = await _dbContext.SaveChangesAsync();
                return result != 0 ? dbUser.Id : -1;
            }
            catch (DbUpdateException e) {
                var callerMethod = new StackTrace().GetFrame(1)?.GetMethod();

                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(UserService)}.{nameof(InsertBlankUserOnAccountRegistration) }",
                    Caller = $"{ callerMethod?.Name }.{ callerMethod?.ReflectedType?.Name }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = "Error while inserting entry into AuthRecords.",
                    ParamData = $"{ nameof(accountId) } = { accountId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }
    }
}