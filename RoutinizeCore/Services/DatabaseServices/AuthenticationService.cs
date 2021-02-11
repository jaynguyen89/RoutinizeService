using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using HelperLibrary;
using HelperLibrary.Shared;
using Microsoft.EntityFrameworkCore;
using MongoLibrary.Interfaces;
using MongoLibrary.Models;
using Newtonsoft.Json;
using RoutinizeCore.DbContexts;
using RoutinizeCore.Models;
using RoutinizeCore.Services.Interfaces;
using RoutinizeCore.ViewModels.Authentication;

namespace RoutinizeCore.Services.DatabaseServices {

    public sealed class AuthenticationService : DbServiceBase, IAuthenticationService {

        public AuthenticationService(
            IRoutinizeCoreLogService coreLogService,
            RoutinizeDbContext dbContext
        ) : base(coreLogService, dbContext) { }
        
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

        public async Task<int> InsertNewUserAccount(
            [NotNull] RegisterAccountVM registrationData,[NotNull] string uniqueId,[NotNull] string activationToken
        ) {
            var newDbAccount = new Account {
                Email = registrationData.Email,
                EmailConfirmed = false,
                Username = registrationData.Username,
                UniqueId = uniqueId,
                PasswordHash = registrationData.Password,
                PasswordSalt = registrationData.PasswordConfirm,
                RecoveryToken = activationToken,
                TokenSetOn = DateTime.UtcNow
            };

            try {
                await _dbContext.Accounts.AddAsync(newDbAccount);
                var result = await _dbContext.SaveChangesAsync();

                return result < 1 ? 0 : newDbAccount.Id;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AuthenticationService) }.{ nameof(InsertNewUserAccount) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Unable to insert new entry into database.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(registrationData) } = { JsonConvert.SerializeObject(registrationData) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return -1;
            }
        }

        public async Task<KeyValuePair<bool, bool?>> ActivateUserAccount([NotNull] AccountActivationVM activator) {
            try {
                var dbAccount = await _dbContext.Accounts.SingleOrDefaultAsync(
                    account => account.Email.ToLower().Equals(activator.Email.Trim().ToLower()) &&
                               account.EmailConfirmed == false &&
                               account.RecoveryToken.Equals(activator.ActivationToken)
                );

                if (dbAccount == null) return new KeyValuePair<bool, bool?>(false, null);

                if (dbAccount.TokenSetOn == null || 
                    dbAccount.TokenSetOn.Value.AddHours(SharedConstants.ACCOUNT_ACTIVATION_EMAIL_VALIDITY_DURATION) <= DateTime.UtcNow
                )
                    return new KeyValuePair<bool, bool?>(false, true);
                
                dbAccount.EmailConfirmed = true;
                dbAccount.RecoveryToken = null;
                dbAccount.TokenSetOn = null;

                _dbContext.Accounts.Update(dbAccount);
                var result = await _dbContext.SaveChangesAsync();

                return new KeyValuePair<bool, bool?>(true, result != 0);
            }
            catch (InvalidOperationException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AuthenticationService) }.{ nameof(ActivateUserAccount) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Error while retrieving an entry with SingleOrDefault, >1 entry matching predicate.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(activator) } = { JsonConvert.SerializeObject(activator) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return new KeyValuePair<bool, bool?>(false, null);
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AuthenticationService) }.{ nameof(ActivateUserAccount) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Unhandled NULL argument passed to database query while reading data with SingleOrDefault.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(activator) } = { JsonConvert.SerializeObject(activator) }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return new KeyValuePair<bool, bool?>(false, false);
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AuthenticationService) }.{ nameof(ActivateUserAccount) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = "Error while updating entry to database.\n\n" + e.StackTrace,
                    ParamData = $"{ nameof(activator) } = { JsonConvert.SerializeObject(activator) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return new KeyValuePair<bool, bool?>(true, null);
            }
        }

        public async Task<KeyValuePair<bool, Account>> AuthenticateUserAccount([NotNull] AuthenticationVM authenticationData) {
            try {
                Account dbAccount = await _dbContext.Accounts.SingleOrDefaultAsync(
                    account => (
                                   Helpers.IsProperString(authenticationData.Email)
                                       ? account.Email.ToLower().Equals(authenticationData.Email.ToLower())
                                       : account.Username.ToLower().Equals(authenticationData.Username.ToLower())
                               ) &&
                               account.EmailConfirmed == true &&
                               account.RecoveryToken == null &&
                               account.TokenSetOn == null
                );

                return dbAccount == null
                    ? new KeyValuePair<bool, Account>(true, null)
                    : new KeyValuePair<bool, Account>(true, dbAccount);
            }
            catch (InvalidOperationException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AuthenticationService) }.{ nameof(AuthenticateUserAccount) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Error while retrieving an entry with SingleOrDefault, >1 entry matching predicate.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(authenticationData) } = { JsonConvert.SerializeObject(authenticationData) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return new KeyValuePair<bool, Account>(false, null);
            }
        }

        public async Task<bool?> InsertAuthenticationRecord([NotNull] AuthRecord authRecord) {
            try {
                await _dbContext.AuthRecords.AddAsync(authRecord);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AuthenticationService) }.{ nameof(InsertAuthenticationRecord) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while inserting entry into AuthRecords.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(authRecord.AccountId) } = { authRecord.AccountId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<AuthRecord> GetLatestAuthRecordForUserAccount([NotNull] int accountId) {
            try {
                var authRecord = await _dbContext.AuthRecords
                                                 .Where(record => record.AccountId == accountId)
                                                 .OrderByDescending(record => record.AuthTimestamp)
                                                 .FirstOrDefaultAsync();

                return authRecord;
            }
            catch (InvalidOperationException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AuthenticationService) }.{ nameof(GetLatestAuthRecordForUserAccount) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Error while retrieving an entry with SingleOrDefault, >1 entry matching predicate.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(accountId) } = { accountId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AuthenticationService) }.{ nameof(ActivateUserAccount) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Unhandled NULL argument passed to database query while reading data with SingleOrDefault.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(accountId) } = { accountId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<Account> GetAccountById([NotNull] int accountId) {
            return await _dbContext.Accounts.FindAsync(accountId);
        }

        public async Task RevokeAuthRecord(int accountId) {
            try {
                var authRecord = await _dbContext.AuthRecords
                                                 .Where(record => record.AccountId == accountId)
                                                 .OrderByDescending(record => record.AuthTimestamp)
                                                 .FirstOrDefaultAsync();

                _dbContext.AuthRecords.Remove(authRecord);
                await _dbContext.SaveChangesAsync();
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AuthenticationService) }.{ nameof(RevokeAuthRecord) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Unhandled NULL argument passed to database query while reading data with FirstOrDefault.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(accountId) } = { accountId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AuthenticationService) }.{ nameof(RevokeAuthRecord) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while removing entry from AuthRecords.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(accountId) } = { accountId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });
            }
        }
    }
}