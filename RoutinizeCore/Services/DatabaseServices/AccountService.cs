using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        private readonly IRoutinizeCoreLogService _coreLogService;
        private readonly RoutinizeDbContext _dbContext;

        public AccountService(
            IRoutinizeCoreLogService coreLogService,
            RoutinizeDbContext dbContext
        ) {
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
                    DetailedInformation = $"Search an entry with SingleOrDefaultAsync, detect 2 entries matching the predicate.\n\n{ e.StackTrace }",
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
                    DetailedInformation = $"Search an entry with SingleOrDefaultAsync, detect 2 entries matching the predicate.\n\n{ e.StackTrace }",
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
                    DetailedInformation = $"Search an entry with SingleOrDefaultAsync, detect 2 entries matching the predicate.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(accountUniqueId) } = { accountUniqueId }",
                    Severity = SharedEnums.LogSeverity.Fatal.GetEnumValue()
                });

                return false;
            }
        }

        public async Task<Account> GetUserAccountByEmail(string email, bool activated = false) {
            try {
                return await _dbContext.Accounts
                                       .SingleOrDefaultAsync(
                                           account => account.Email.ToLower().Equals(email.Trim().ToLower()) &&
                                                      account.EmailConfirmed == activated
                                        );
            }
            catch (InvalidOperationException e) {
                var callerMethod = new StackTrace().GetFrame(1)?.GetMethod();
                
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AccountService) }.{ nameof(GetUserAccountByEmail) }",
                    Caller = $"{ callerMethod?.Name }.{ callerMethod?.ReflectedType?.Name }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Search an entry with SingleOrDefaultAsync, detect 2 entries matching the predicate.\n\n{ e.StackTrace }",
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
                    DetailedInformation = $"An error occurred while updating entry to database, either concurrency or integrity conflict.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(userAccount) } = { JsonConvert.SerializeObject(userAccount) }",
                    Severity = SharedEnums.LogSeverity.Fatal.GetEnumValue()
                });

                return false;
            }
        }

        public async Task<Account> GetUserAccountById(int accountId, bool activated = true) {
            var cachedAccount = GetMemoryCacheEntry<Account>();
            if (cachedAccount != null) return cachedAccount;

            Account dbAccount = null;
            try {
                dbAccount = await _dbContext.Accounts.Where(account =>
                    account.Id == accountId &&
                    account.EmailConfirmed == activated
                ).FirstOrDefaultAsync();
            }
            catch (ArgumentNullException e) {
                var callerMethod = new StackTrace().GetFrame(1)?.GetMethod();
                
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AccountService) }.{ nameof(GetUserAccountById) }",
                    Caller = $"{ callerMethod?.Name }.{ callerMethod?.ReflectedType?.Name }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"An error occurred while getting account by Id.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(accountId) } = { accountId }, { nameof(activated) } = { activated }",
                    Severity = SharedEnums.LogSeverity.Fatal.GetEnumValue()
                });

                return null;
            }

            if (dbAccount != null)
                InsertMemoryCacheEntry<Account>(new CacheEntry {
                    Data = dbAccount,
                    Priority = CacheItemPriority.High,
                    Size = dbAccount.GetType().GetProperties().Length,
                    AbsoluteExpiration = SharedConstants.CACHE_ABSOLUTE_EXPIRATION
                });

            return dbAccount;
        }

        public async Task<Account> GetUserAccountByUsername(string username, bool activated = false) {
            try {
                return await _dbContext.Accounts.Where(account =>
                    account.Username.ToLower().Equals(username) &&
                    account.EmailConfirmed == activated
                ).FirstOrDefaultAsync();
            }
            catch (ArgumentNullException e) {
                var callerMethod = new StackTrace().GetFrame(1)?.GetMethod();
                
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AccountService) }.{ nameof(GetUserAccountByUsername) }",
                    Caller = $"{ callerMethod?.Name }.{ callerMethod?.ReflectedType?.Name }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"An error occurred while getting account by username.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(username) } = { username }, { nameof(activated) } = { activated }",
                    Severity = SharedEnums.LogSeverity.Fatal.GetEnumValue()
                });

                return null;
            }
        }
    }
}