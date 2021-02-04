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

namespace RoutinizeCore.Services.DatabaseServices {

    public sealed class UserService : IUserService {
        
        private readonly IRoutinizeCoreLogService _coreLogService;
        private readonly RoutinizeDbContext _dbContext;

        public UserService(
            IRoutinizeCoreLogService coreLogService,
            RoutinizeDbContext dbContext
        ) {
            _coreLogService = coreLogService;
            _dbContext = dbContext;
        }

        public async Task<int?> InsertBlankUserWithPrivacyAndAppSetting([NotNull] int accountId) {
            try {
                var dbUser = new User { AccountId = accountId };
                await _dbContext.Users.AddAsync(dbUser);

                var saveUserResult = await _dbContext.SaveChangesAsync();
                if (saveUserResult == 0) return -1;

                await _dbContext.UserPrivacies.AddAsync(new UserPrivacy { UserId = dbUser.Id });
                await _dbContext.AppSettings.AddAsync(new AppSetting { UserId = dbUser.Id });

                var saveResult = await _dbContext.SaveChangesAsync();
                return saveResult == 0 ? -1 : dbUser.Id;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(UserService) }.{ nameof(InsertBlankUserWithPrivacyAndAppSetting) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while inserting entry into Users.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(accountId) } = { accountId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<KeyValuePair<bool, User>> GetUserProfileByAccountId([NotNull] int accountId) {
            try {
                var userProfile = await _dbContext.Users.SingleOrDefaultAsync(user => user.AccountId == accountId);
                return new KeyValuePair<bool, User>(false, userProfile);
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(UserService) }.{ nameof(GetUserProfileByAccountId) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while getting user by SingleOrDefault due to Null argument.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(accountId) } = { accountId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return new KeyValuePair<bool, User>(true, null);
            }
            catch (InvalidOperationException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(UserService) }.{ nameof(GetUserProfileByAccountId) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Error while getting user, >1 entry matches predicate.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(accountId) } = { accountId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return new KeyValuePair<bool, User>(true, null);
            }
        }

        public async Task<bool?> UpdateUserProfile([NotNull] User userProfile) {
            try {
                _dbContext.Users.Update(userProfile);
                var result = await _dbContext.SaveChangesAsync();
                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(UserService) }.{ nameof(UpdateUserProfile) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while updating entry to Users.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(userProfile) } = { JsonConvert.SerializeObject(userProfile) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<int?> SaveNewUserProfile([NotNull] User userProfile) {
            try {
                await _dbContext.Users.AddAsync(userProfile);
                var result = await _dbContext.SaveChangesAsync();
                
                return result == 0 ? -1 : userProfile.Id;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(UserService) }.{ nameof(SaveNewUserProfile) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while inserting entry to Users.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(userProfile) } = { JsonConvert.SerializeObject(userProfile) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> UpdateUserPrivacy([NotNull] UserPrivacy userPrivacy) {
            var userPrivacyId = await GetUserPrivacyOrAppSettingIdByUserId(userPrivacy.UserId);
            if (userPrivacyId == null) return null;

            userPrivacy.Id = userPrivacyId.Value;
            _dbContext.UserPrivacies.Update(userPrivacy);
            try {
                var result = await _dbContext.SaveChangesAsync();
                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(UserService) }.{ nameof(UpdateUserPrivacy) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while updating entry to UserPrivacies.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(userPrivacy) } = { JsonConvert.SerializeObject(userPrivacy) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> UpdateUserAppSettings([NotNull] AppSetting appSetting) {
            var appSettingId = await GetUserPrivacyOrAppSettingIdByUserId(appSetting.UserId, nameof(AppSetting));
            if (appSettingId == null) return null;

            appSetting.Id = appSettingId.Value;
            _dbContext.AppSettings.Update(appSetting);
            try {
                var result = await _dbContext.SaveChangesAsync();
                return result != 0;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"{ nameof(UserService) }.{ nameof(UpdateUserAppSettings) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while updating entry to AppSettings.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(appSetting) } = { JsonConvert.SerializeObject(appSetting) }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> IsUserProfileCreated([NotNull] int accountId) {
            try {
                return await _dbContext.Users.AnyAsync(user => user.AccountId == accountId);
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(UserService) }.{ nameof(IsUserProfileCreated) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while getting data from Users.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(accountId) } = { accountId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<KeyValuePair<bool, UserPrivacy>> GetUserPrivacy([NotNull] int accountId) {
            try {
                var userPrivacy = await _dbContext.Users
                                                  .Where(user => user.AccountId == accountId)
                                                  .Join(
                                                      _dbContext.UserPrivacies,
                                                      user => user.Id,
                                                      privacy => privacy.UserId,
                                                      ((user, privacy) => new {user, privacy})
                                                  )
                                                  .Select(joinedUserPrivacy => joinedUserPrivacy.privacy)
                                                  .SingleOrDefaultAsync();

                return new KeyValuePair<bool, UserPrivacy>(true, userPrivacy);
            }
            catch (InvalidOperationException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(UserService) }.{ nameof(GetUserPrivacy) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Error while getting entry from UserPrivacies.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(accountId) } = { accountId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return new KeyValuePair<bool, UserPrivacy>(false, null);
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(UserService) }.{ nameof(GetUserPrivacy) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while getting entry from UserPrivacies.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(accountId) } = { accountId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return new KeyValuePair<bool, UserPrivacy>(false, null);
            }
        }

        public async Task<KeyValuePair<bool, AppSetting>> GetAppSettings([NotNull] int accountId) {
            try {
                var appSetting = await _dbContext.Users
                                                 .Where(user => user.AccountId == accountId)
                                                 .Join(
                                                     _dbContext.AppSettings,
                                                     user => user.Id,
                                                     setting => setting.UserId,
                                                     ((user, setting) => new {user, setting})
                                                 )
                                                 .Select(joinedUserSetting => joinedUserSetting.setting)
                                                 .SingleOrDefaultAsync();
                
                return new KeyValuePair<bool, AppSetting>(true, appSetting);
            }
            catch (InvalidOperationException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(UserService) }.{ nameof(GetAppSettings) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(InvalidOperationException),
                    DetailedInformation = $"Error while getting entry from AppSettings.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(accountId) } = { accountId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return new KeyValuePair<bool, AppSetting>(false, null);
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(UserService) }.{ nameof(GetAppSettings) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while getting entry from AppSettings.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(accountId) } = { accountId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return new KeyValuePair<bool, AppSetting>(false, null);
            }
        }

        public async Task<bool?> CheckIfUserProfileInitialized(int accountId) {
            try {
                return await _dbContext.Users.AnyAsync(user => user.AccountId == accountId);
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(UserService) }.{ nameof(CheckIfUserProfileInitialized) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while checking entry existed in Users with AnyAsync.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(accountId) } = { accountId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        public async Task<bool?> DoesUserHasPremiumOrTodoUnlocked(int userId) {
            try {
                return await _dbContext.AppSettings.AnyAsync(settings => settings.UserId == userId && (settings.IsPremium || settings.TodoUnlocked));
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(UserService) }.{ nameof(DoesUserHasPremiumOrTodoUnlocked) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while checking entry existed in Users with AnyAsync.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(userId) } = { userId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }

        private async Task<int?> GetUserPrivacyOrAppSettingIdByUserId([NotNull] int userId, string assetType = nameof(UserPrivacy)) {
            try {
                return assetType.Equals(nameof(UserPrivacy))
                    ? (await _dbContext.UserPrivacies.SingleOrDefaultAsync(privacy => privacy.UserId == userId)).Id
                    : (await _dbContext.AppSettings.SingleOrDefaultAsync(setting => setting.UserId == userId)).Id;
            }
            catch (DbUpdateException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(UserService) }.{ nameof(GetUserPrivacyOrAppSettingIdByUserId) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(DbUpdateException),
                    DetailedInformation = $"Error while getting entry from UserPrivacies.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(userId) } = { userId }",
                    Severity = SharedEnums.LogSeverity.High.GetEnumValue()
                });

                return null;
            }
            catch (ArgumentNullException e) {
                await _coreLogService.InsertRoutinizeCoreLog(new RoutinizeCoreLog {
                    Location = $"private { nameof(UserService) }.{ nameof(GetUserPrivacyOrAppSettingIdByUserId) }",
                    Caller = $"{ new StackTrace().GetFrame(4)?.GetMethod()?.DeclaringType?.FullName }",
                    BriefInformation = nameof(ArgumentNullException),
                    DetailedInformation = $"Error while getting entry from UserPrivacies.\n\n{ e.StackTrace }",
                    ParamData = $"{ nameof(userId) } = { userId }",
                    Severity = SharedEnums.LogSeverity.Caution.GetEnumValue()
                });

                return null;
            }
        }
    }
}