using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using HelperLibrary;
using HelperLibrary.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MongoLibrary.Interfaces;
using MongoLibrary.Models;
using Newtonsoft.Json;
using RoutinizeCore.DbContexts;
using RoutinizeCore.Models;
using RoutinizeCore.Services.ApplicationServices.CacheService;
using RoutinizeCore.Services.Interfaces;

namespace RoutinizeCore.Services.DatabaseServices {

    public sealed class AccountService : CacheServiceBase, IAccountService {

        private readonly IRoutinizeMemoryCache _memoryCache;
        private readonly IRoutinizeCoreLogService _coreLogService;
        private readonly RoutinizeDbContext _dbContext;

        public AccountService(
            IRoutinizeMemoryCache memoryCache,
            IRoutinizeCoreLogService coreLogService,
            RoutinizeDbContext dbContext
        ) {
            _memoryCache = memoryCache;
            _coreLogService = coreLogService;
            _dbContext = dbContext;
        }

        public async Task<bool> IsRegistrationEmailAvailable(string email) {
            try {
                var dbAccount = await _dbContext.Accounts.SingleOrDefaultAsync(account => account.Email.ToLower().Equals(email));
                return dbAccount == null;
            }
            catch (InvalidOperationException e) {
                var callerMethod = new StackTrace().GetFrame(1)?.GetMethod();
                
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AccountService) }.{ nameof(IsRegistrationEmailAvailable) }",
                    Caller = $"{ callerMethod?.Name }.{ callerMethod?.ReflectedType?.Name }",
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
                var dbAccount = await _dbContext.Accounts.SingleOrDefaultAsync(account => account.Username.ToLower().Equals(username));
                return dbAccount == null;
            }
            catch (InvalidOperationException e) {
                var callerMethod = new StackTrace().GetFrame(1)?.GetMethod();
                
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AccountService) }.{ nameof(IsUsernameAvailable) }",
                    Caller = $"{ callerMethod?.Name }.{ callerMethod?.ReflectedType?.Name }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = "Search an entry with SingleOrDefaultAsync, detect 2 entries matching the predicate.",
                    ParamData = $"{ nameof(username) } = { username }",
                    Severity = SharedEnums.LogSeverity.Fatal.GetEnumValue()
                });

                return false;
            }
        }

        public async Task<bool> IsAccountUniqueIdAvailable(string accountUniqueId) {
            try {
                var dbAccount = await _dbContext.Accounts.SingleOrDefaultAsync(account =>
                    Helpers.RemoveCharactersFromString(
                        account.UniqueId,
                        new List<char> {SharedConstants.ACCOUNT_UNIQUE_ID_DELIMITER}
                    ).ToUpper().Equals(accountUniqueId.ToUpper())
                );

                return dbAccount == null;
            }
            catch (InvalidOperationException e) {
                var callerMethod = new StackTrace().GetFrame(1)?.GetMethod();
                
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AccountService) }.{ nameof(IsAccountUniqueIdAvailable) }",
                    Caller = $"{ callerMethod?.Name }.{ callerMethod?.ReflectedType?.Name }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = "Search an entry with SingleOrDefaultAsync, detect 2 entries matching the predicate.",
                    ParamData = $"{ nameof(accountUniqueId) } = { accountUniqueId }",
                    Severity = SharedEnums.LogSeverity.Fatal.GetEnumValue()
                });

                return false;
            }
        }

        public async Task<Account> GetUnactivatedUserAccountByEmail(string email) {
            try {
                return await _dbContext.Accounts
                                       .SingleOrDefaultAsync(
                                           account => account.Email.ToLower().Equals(email.Trim().ToLower()) &&
                                                      account.EmailConfirmed == false
                                        );
            }
            catch (InvalidOperationException e) {
                var callerMethod = new StackTrace().GetFrame(1)?.GetMethod();
                
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AccountService) }.{ nameof(GetUnactivatedUserAccountByEmail) }",
                    Caller = $"{ callerMethod?.Name }.{ callerMethod?.ReflectedType?.Name }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = "Search an entry with SingleOrDefaultAsync, detect 2 entries matching the predicate.",
                    ParamData = $"{ nameof(email) } = { email }",
                    Severity = SharedEnums.LogSeverity.Fatal.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool> UpdateUserAccount(Account userAccount) {
            try {
                _dbContext.Accounts.Update(userAccount);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (DbUpdateException e) {
                var callerMethod = new StackTrace().GetFrame(1)?.GetMethod();
                
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AccountService) }.{ nameof(UpdateUserAccount) }",
                    Caller = $"{ callerMethod?.Name }.{ callerMethod?.ReflectedType?.Name }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = "An error occurred while updating entry to database, either concurrency or integrity conflict.",
                    ParamData = $"{ nameof(userAccount) } = { JsonConvert.SerializeObject(userAccount) }",
                    Severity = SharedEnums.LogSeverity.Fatal.GetEnumValue()
                });

                return false;
            }
        }

        public async Task<Account> GetAccountById(int accountId) {
            var cachedAccount = _memoryCache.GetCacheEntryFor<Account>($"{ nameof(Account) }_{ accountId }");
            if (cachedAccount != null) return cachedAccount;
            
            var dbAccount = await _dbContext.Accounts.FindAsync(accountId);
            if (dbAccount != null)
                _memoryCache.SetCacheEntry(new CacheEntry {
                    Data = dbAccount,
                    Priority = CacheItemPriority.High,
                    Size = dbAccount.GetType().GetProperties().Length,
                    AbsoluteExpiration = SharedConstants.CACHE_ABSOLUTE_EXPIRATION,
                    EntryKey = $"{ nameof(Account) }_{ accountId }"
                });

            return dbAccount;
        }
    }
}