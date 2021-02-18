using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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

    public sealed class AccountService : DbServiceBase, IAccountService {

        private readonly IRoutinizeMemoryCache _memoryCache;

        public AccountService(
            IRoutinizeCoreLogService coreLogService,
            RoutinizeDbContext dbContext,
            IRoutinizeMemoryCache memoryCache
        ) : base(coreLogService, dbContext) {
            _memoryCache = memoryCache;
        }
        
        public new async Task SetChangesToDbContext(object any, string task = SharedConstants.TASK_INSERT) {
            await base.SetChangesToDbContext(any, task);
        }

        public new async Task<bool?> CommitChanges() {
            return await base.CommitChanges();
        }

        public new void ToggleTransactionAuto(bool auto = true) {
            base.ToggleTransactionAuto(auto);
        }

        public new async Task StartTransaction() {
            await base.StartTransaction();
        }

        public new async Task CommitTransaction() {
            await base.CommitTransaction();
        }

        public new async Task RevertTransaction() {
            await base.RevertTransaction();
        }

        public new async Task ExecuteRawOn<T>(string query) {
            await base.ExecuteRawOn<T>(query);
        }

        public async Task<bool> IsRegistrationEmailAvailable([NotNull] string email) {
            try {
                var dbAccount = await _dbContext.Accounts.SingleOrDefaultAsync(account => account.Email.ToLower().Equals(email));
                return dbAccount == null;
            }
            catch (InvalidOperationException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AccountService) }.{ nameof(IsRegistrationEmailAvailable) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Search an entry with SingleOrDefaultAsync, detect 2 entries matching the predicate.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(email) } = { email }",
                    Severity = SharedEnums.LogSeverity.Fatal.GetEnumValue()
                });

                return false;
            }
        }

        public async Task<bool> IsUsernameAvailable([NotNull] string username) {
            try {
                var dbAccount = await _dbContext.Accounts.SingleOrDefaultAsync(account => account.Username.ToLower().Equals(username));
                return dbAccount == null;
            }
            catch (InvalidOperationException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AccountService) }.{ nameof(IsUsernameAvailable) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Search an entry with SingleOrDefaultAsync, detect 2 entries matching the predicate.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(username) } = { username }",
                    Severity = SharedEnums.LogSeverity.Fatal.GetEnumValue()
                });

                return false;
            }
        }

        public async Task<bool> IsAccountUniqueIdAvailable([NotNull] string accountUniqueId) {
            try {
                return await _dbContext.Accounts.AnyAsync(account =>
                    account.UniqueId.Equals(accountUniqueId.ToUpper())
                );
            }
            catch (ArgumentNullException) {
                return false;
            }
        }

        public async Task<Account> GetUserAccountByEmail([NotNull] string email, bool activated = false) {
            try {
                return await _dbContext.Accounts
                                       .SingleOrDefaultAsync(
                                           account => account.Email.ToLower().Equals(email.Trim().ToLower()) &&
                                                      account.EmailConfirmed == activated
                                        );
            }
            catch (InvalidOperationException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AccountService) }.{ nameof(GetUserAccountByEmail) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Search an entry with SingleOrDefaultAsync, detect 2 entries matching the predicate.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(email) } = { email }",
                    Severity = SharedEnums.LogSeverity.Fatal.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool> UpdateUserAccount([NotNull] Account userAccount) {
            try {
                _dbContext.Accounts.Update(userAccount);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AccountService) }.{ nameof(UpdateUserAccount) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"An error occurred while updating entry to database, either concurrency or integrity conflict.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(userAccount) } = { JsonConvert.SerializeObject(userAccount) }",
                    Severity = SharedEnums.LogSeverity.Fatal.GetEnumValue()
                });

                return false;
            }
        }

        public async Task<Account> GetUserAccountById([NotNull] int accountId, bool activated = true) {
            var cachedAccount = _memoryCache.GetCacheEntryFor<Account>($"{ nameof(Account) }_{ accountId }");
            if (cachedAccount != null) return cachedAccount;

            Account dbAccount = null;
            try {
                dbAccount = await _dbContext.Accounts.Where(account =>
                    account.Id == accountId &&
                    account.EmailConfirmed == activated
                ).FirstOrDefaultAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AccountService) }.{ nameof(GetUserAccountById) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"An error occurred while getting account by Id.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(accountId) } = { accountId }, { nameof(activated) } = { activated }",
                    Severity = SharedEnums.LogSeverity.Fatal.GetEnumValue()
                });

                return null;
            }

            if (dbAccount != null)
                _memoryCache.SetCacheEntry<Account>(new CacheEntry {
                    EntryKey = $"{ nameof(Account) }_{ accountId }",
                    Data = dbAccount,
                    Priority = CacheItemPriority.Normal,
                    Size = dbAccount.GetType().GetProperties().Length
                });

            return dbAccount;
        }

        public async Task<Account> GetUserAccountByUsername([NotNull] string username, bool activated = false) {
            try {
                return await _dbContext.Accounts.Where(account =>
                    account.Username.ToLower().Equals(username) &&
                    account.EmailConfirmed == activated
                ).FirstOrDefaultAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AccountService) }.{ nameof(GetUserAccountByUsername) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"An error occurred while getting account by username.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(username) } = { username }, { nameof(activated) } = { activated }",
                    Severity = SharedEnums.LogSeverity.Fatal.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> SaveFcmToken([NotNull] int accountId,[NotNull] string fcmToken) {
            try {
                var dbAccount = await _dbContext.Accounts.FindAsync(accountId);
                dbAccount.FcmToken = fcmToken;

                _dbContext.Accounts.Update(dbAccount);
                var result = await _dbContext.SaveChangesAsync();
                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AccountService) }.{ nameof(SaveFcmToken) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"An error occurred while updating entry to database, either concurrency or integrity conflict.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(accountId) } = { accountId }",
                    Severity = SharedEnums.LogSeverity.Fatal.GetEnumValue()
                });

                return false;
            }
        }
    }
}