﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    public sealed class AuthenticationService : CacheServiceBase, IAuthenticationService {
        
        private readonly IRoutinizeCoreLogService _coreLogService;
        private readonly RoutinizeDbContext _dbContext;

        public AuthenticationService(
            IRoutinizeCoreLogService coreLogService,
            RoutinizeDbContext dbContext
        ) {
            _coreLogService = coreLogService;
            _dbContext = dbContext;
        }

        public async Task<int> InsertNewUserAccount(
            RegisterAccountVM registrationData, string uniqueId, string activationToken
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
                var callerMethod = new StackTrace().GetFrame(1)?.GetMethod();
                
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AuthenticationService) }.{ nameof(InsertNewUserAccount) }",
                    Caller = $"{ callerMethod?.Name }.{ callerMethod?.ReflectedType?.Name }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Unable to insert new entry into database.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(registrationData) } = { JsonConvert.SerializeObject(registrationData) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return -1;
            }
        }

        public async Task<bool> RemoveNewlyInsertedUserAccount(int accountId) {
            try {
                var dbAccountToRemove = await _dbContext.Accounts.FindAsync(accountId);
                _dbContext.Accounts.Remove(dbAccountToRemove);

                var result = await _dbContext.SaveChangesAsync();
                return result > 0;
            }
            catch (DbUpdateException e) {
                var callerMethod = new StackTrace().GetFrame(1)?.GetMethod();
                
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AuthenticationService) }.{ nameof(RemoveNewlyInsertedUserAccount) }",
                    Caller = $"{ callerMethod?.Name }.{ callerMethod?.ReflectedType?.Name }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Failed to remove an instance from database table Accounts.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(accountId) } = { accountId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return false;
            }
        }

        public async Task<KeyValuePair<bool, bool?>> ActivateUserAccount(AccountActivationVM activator) {
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
                var callerMethod = new StackTrace().GetFrame(1)?.GetMethod();

                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AuthenticationService) }.{ nameof(ActivateUserAccount) }",
                    Caller = $"{ callerMethod?.Name }.{ callerMethod?.ReflectedType?.Name }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Error while retrieving an entry with SingleOrDefault, >1 entry matching predicate.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(activator) } = { JsonConvert.SerializeObject(activator) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return new KeyValuePair<bool, bool?>(false, null);
            }
            catch (ArgumentNullException e) {
                var callerMethod = new StackTrace().GetFrame(1)?.GetMethod();

                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AuthenticationService) }.{ nameof(ActivateUserAccount) }",
                    Caller = $"{ callerMethod?.Name }.{ callerMethod?.ReflectedType?.Name }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Unhandled NULL argument passed to database query while reading data with SingleOrDefault.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(activator) } = { JsonConvert.SerializeObject(activator) }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return new KeyValuePair<bool, bool?>(false, false);
            }
            catch (DbUpdateException e) {
                var callerMethod = new StackTrace().GetFrame(1)?.GetMethod();

                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AuthenticationService) }.{ nameof(ActivateUserAccount) }",
                    Caller = $"{ callerMethod?.Name }.{ callerMethod?.ReflectedType?.Name }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = "Error while updating entry to database.\n\n" + e.StackTrace,
                    ParamData = $"{ nameof(activator) } = { JsonConvert.SerializeObject(activator) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return new KeyValuePair<bool, bool?>(true, null);
            }
        }

        public async Task<KeyValuePair<bool, Account>> AuthenticateUserAccount(AuthenticationVM authenticationData) {
            try {
                Account dbAccount = await _dbContext.Accounts.SingleOrDefaultAsync(
                    account => (
                                   Helpers.IsProperString(authenticationData.Email)
                                       ? account.Email.ToLower().Equals(authenticationData.Email.ToLower())
                                       : account.Username.ToLower().Equals(authenticationData.Username)
                               ) &&
                               account.EmailConfirmed == true &&
                               account.RecoveryToken == null &&
                               account.TokenSetOn == null
                );

                return dbAccount == null
                    ? new KeyValuePair<bool, Account>(false, null)
                    : new KeyValuePair<bool, Account>(true, dbAccount);
            }
            catch (InvalidOperationException e) {
                var callerMethod = new StackTrace().GetFrame(1)?.GetMethod();

                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AuthenticationService)}.{nameof(AuthenticateUserAccount) }",
                    Caller = $"{ callerMethod?.Name }.{ callerMethod?.ReflectedType?.Name }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Error while retrieving an entry with SingleOrDefault, >1 entry matching predicate.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(authenticationData) } = { JsonConvert.SerializeObject(authenticationData) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return new KeyValuePair<bool, Account>(false, null);
            }
        }

        public async Task<bool?> InsertAuthenticationRecord(AuthRecord authRecord) {
            try {
                await _dbContext.AuthRecords.AddAsync(authRecord);
                var result = await _dbContext.SaveChangesAsync();

                return result != 0;
            }
            catch (DbUpdateException e) {
                var callerMethod = new StackTrace().GetFrame(1)?.GetMethod();

                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AuthenticationService)}.{nameof(InsertAuthenticationRecord) }",
                    Caller = $"{ callerMethod?.Name }.{ callerMethod?.ReflectedType?.Name }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while inserting entry into AuthRecords.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(authRecord) } = { JsonConvert.SerializeObject(authRecord) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<AuthRecord> GetLatestAuthRecordForUserAccount(SessionAuthVM sessionAuth) {
            try {
                var authRecord = await _dbContext.AuthRecords.SingleOrDefaultAsync(
                    record => record.AccountId == sessionAuth.AccountId &&
                              record.SessionId.Equals(sessionAuth.SessionId) &&
                              record.TrustedAuth == sessionAuth.GetTrustedAuth()
                );

                return authRecord;
            }
            catch (InvalidOperationException e) {
                var callerMethod = new StackTrace().GetFrame(1)?.GetMethod();

                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AuthenticationService) }.{ nameof(GetLatestAuthRecordForUserAccount) }",
                    Caller = $"{ callerMethod?.Name }.{ callerMethod?.ReflectedType?.Name }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Error while retrieving an entry with SingleOrDefault, >1 entry matching predicate.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(sessionAuth) } = { JsonConvert.SerializeObject(sessionAuth) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
            catch (ArgumentNullException e) {
                var callerMethod = new StackTrace().GetFrame(1)?.GetMethod();

                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(AuthenticationService) }.{ nameof(ActivateUserAccount) }",
                    Caller = $"{ callerMethod?.Name }.{ callerMethod?.ReflectedType?.Name }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Unhandled NULL argument passed to database query while reading data with SingleOrDefault.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(sessionAuth) } = { JsonConvert.SerializeObject(sessionAuth) }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<Account> GetAccountById(int accountId) {
            return await _dbContext.Accounts.FindAsync(accountId);
        }
    }
}