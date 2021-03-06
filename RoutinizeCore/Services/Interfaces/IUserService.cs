﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using RoutinizeCore.Models;

namespace RoutinizeCore.Services.Interfaces {

    public interface IUserService : IDbServiceBase {

        Task<int?> InsertBlankUserWithPrivacyAndAppSetting([NotNull] int accountId);

        Task<KeyValuePair<bool, User>> GetUserProfileByAccountId([NotNull] int accountId);

        Task<bool?> UpdateUserProfile([NotNull] User userProfile);

        Task<int?> SaveNewUserProfile([NotNull] User userProfile);

        Task<bool?> UpdateUserPrivacy([NotNull] UserPrivacy userPrivacy);

        Task<bool?> UpdateUserAppSettings([NotNull] AppSetting appSetting);

        Task<bool?> IsUserProfileCreated([NotNull] int accountId);

        Task<KeyValuePair<bool, UserPrivacy>> GetUserPrivacy([NotNull] int accountId);

        Task<KeyValuePair<bool, AppSetting>> GetAppSettings([NotNull] int accountId);

        Task<bool?> CheckIfUserProfileInitialized([NotNull] int accountId);

        Task<bool?> DoesUserHasPremiumOrTodoUnlocked([NotNull] int userId);
        
        Task<User> GetUserByUniqueId([NotNull] string uniqueId);
        
        Task<User> GetUserById([NotNull] int userId);
        
        Task<Account> GetAccountByUserId([NotNull] int userId);
        
        Task<bool?> DoesUserHasPremiumOrNoteUnlocked(int userId);
        
        Task<KeyValuePair<bool, DateTime?>> CheckActiveRsaKey(int userId);
        
        Task<bool?> SaveNewUserRsaKey(int userId, string publicKey, string privateKey);
        
        Task<UserRsaKey> GetUserRsaKeyByUserId(int userId);
        
        Task<bool?> DoesUserHasPremiumForAnything(int userId);
    }
}