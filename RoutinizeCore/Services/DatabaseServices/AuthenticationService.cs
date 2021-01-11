using System;
using System.Linq;
using System.Threading.Tasks;
using HelperLibrary;
using HelperLibrary.Shared;
using MongoLibrary.Interfaces;
using RoutinizeCore.DbContexts;
using RoutinizeCore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using MongoLibrary.Models;
using RoutinizeCore.Controllers;

namespace RoutinizeCore.Services.DatabaseServices {

    public sealed class AuthenticationService : CacheServiceBase, IAuthenticationService {
        
        private readonly IRoutinizeCoreLogService _coreLogService;
        private readonly RoutinizeDbContext _routinizeDbContext;

        public AuthenticationService(
            IRoutinizeCoreLogService coreLogService,
            RoutinizeDbContext routinizeDbContext
        ) {
            _coreLogService = coreLogService;
            _routinizeDbContext = routinizeDbContext;
        }

        public async Task<bool> IsRegistrationEmailAvailable(string email) {
            try {
                var dbAccount = await _routinizeDbContext.Accounts.SingleOrDefaultAsync(account => account.Email.ToLower().Equals(email));
                return dbAccount == null;
            }
            catch (InvalidOperationException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Controller = nameof(AuthenticationController),
                    Action = nameof(AuthenticationController.CheckRegistrationEmailAvailability),
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = "Search an entry with SingleOrDefaultAsync, detect 2 entries matching the predicate.",
                    ParamData = $"{ nameof(email) } = { email }",
                    Severity = SharedEnums.LogSeverity.Fatal.GetEnumValue()
                });

                return false;
            }
        }

        public async Task<bool> IsUsernameAvailable(string username) {
            try {
                var dbAccount = await _routinizeDbContext.Accounts.SingleOrDefaultAsync(account => account.Username.ToLower().Equals(username));
                return dbAccount == null;
            }
            catch (InvalidOperationException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Controller = nameof(AuthenticationController),
                    Action = nameof(AuthenticationController.CheckUsernameAvailability),
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = "Search an entry with SingleOrDefaultAsync, detect 2 entries matching the predicate.",
                    ParamData = $"{ nameof(username) } = { username }",
                    Severity = SharedEnums.LogSeverity.Fatal.GetEnumValue()
                });

                return false;
            }
        }
    }
}