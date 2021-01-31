using System.Threading.Tasks;
using HelperLibrary;
using HelperLibrary.Shared;
using Microsoft.AspNetCore.Mvc;
using RoutinizeCore.Attributes;
using RoutinizeCore.Models;
using RoutinizeCore.Services.Interfaces;
using RoutinizeCore.ViewModels;
using RoutinizeCore.ViewModels.User;

namespace RoutinizeCore.Controllers {

    [ApiController]
    [Route("user")]
    public sealed class UserController {

        private readonly IUserService _userService;
        private readonly IAddressService _addressService;

        public UserController(
            IUserService userService,
            IAddressService addressService
        ) {
            _userService = userService;
            _addressService = addressService;
        }

        [HttpGet("is-user-profile-initialized")]
        [RoutinizeActionFilter]
        public async Task<JsonResult> IsUserProfileInitialized([FromHeader] int accountId) {
            var result = await _userService.CheckIfUserProfileInitialized(accountId);
            return !result.HasValue ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." })
                                    : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = result.Value });
        }

        [HttpGet("initialize-user-profile")]
        [RoutinizeActionFilter]
        public async Task<JsonResult> InitializeUserProfile([FromHeader] int accountId) {
            var result = await _userService.InsertBlankUserWithPrivacyAndAppSetting(accountId);
            return !result.HasValue || result.Value < 1
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = result.Value });
        }

        [HttpGet("profile/{accountId}")]
        [RoutinizeActionFilter]
        public async Task<JsonResult> GetUserProfile(int accountId) {
            var (error, userProfile) = await _userService.GetUserProfileByAccountId(accountId);
            if (error) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            userProfile ??= new User { AccountId = accountId };
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = userProfile });
        }

        [HttpPost("set-profile-avatar")]
        [RoutinizeActionFilter]
        public async Task<JsonResult> SetProfileAvatar(ProfileAvatarVM avatarData) {
            var errors = avatarData.CheckAvatar();
            if (errors.Count != 0) {
                var errorMessages = avatarData.GenerateErrorMessages(errors);
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errorMessages });
            }

            var (error, userProfile) = await _userService.GetUserProfileByAccountId(avatarData.AccountId);
            if (error) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            bool? result;
            if (Helpers.IsProperString(avatarData.AvatarName)) {
                if (userProfile == null) {
                    userProfile = new User {
                        AccountId = avatarData.AccountId,
                        AvatarName = avatarData.AvatarName
                    };

                    var saveResult = await _userService.SaveNewUserProfile(userProfile);
                    result = saveResult > 0;
                }
                else {
                    userProfile.AvatarName = avatarData.AvatarName;
                    result = await _userService.UpdateUserProfile(userProfile);
                }

                return !result.HasValue || !result.Value
                    ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." })
                    : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
            }
            
            //TODO: Need to implement uploading AvatarFile later
            return null;
        }

        [HttpPost("update-profile")]
        [RoutinizeActionFilter]
        public async Task<JsonResult> UpdateUserProfile(User profile) {
            var errors = profile.VerifyProfileData();
            if (errors.Count != 0) {
                var errorMessages = profile.GenerateErrorMessage(errors);
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errorMessages });
            }

            var (error, userProfile) = await _userService.GetUserProfileByAccountId(profile.AccountId);
            if (error) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            bool? result;
            if (userProfile == null) {
                var saveResult = await _userService.SaveNewUserProfile(profile);
                result = saveResult > 0;
            }
            else {
                userProfile.FirstName = profile.FirstName;
                userProfile.LastName = profile.LastName;
                userProfile.PreferredName = profile.PreferredName;
                userProfile.Gender = profile.Gender;

                result = await _userService.UpdateUserProfile(userProfile);
            }

            return !result.HasValue || !result.Value
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }

        [HttpPost("save-user-address")]
        [RoutinizeActionFilter]
        public async Task<JsonResult> SaveUserAddress([FromRoute] int accountId, [FromBody] Address address) {
            var errors = address.VerifyAddressData();
            if (errors.Count != 0) {
                var errorMessages = address.GenerateErrorMessages(errors);
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errorMessages });
            }
            
            var (error, userProfile) = await _userService.GetUserProfileByAccountId(accountId);
            if (error) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            if (userProfile == null) {
                var saveProfileResult = await _userService.SaveNewUserProfile(new User { AccountId = accountId });
                if (!saveProfileResult.HasValue || saveProfileResult.Value < 1)
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });

                userProfile = new User { Id = saveProfileResult.Value, AccountId = accountId };
            }

            var saveAddressResult = await _addressService.SaveNewAddress(address);
            if (!saveAddressResult.HasValue || saveAddressResult.Value < 1)
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });

            userProfile.AddressId = saveAddressResult;
            var updateResult = await _userService.UpdateUserProfile(userProfile);

            return updateResult.HasValue && updateResult.Value
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = userProfile })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
        }

        [HttpPut("update-user-address")]
        [RoutinizeActionFilter]
        public async Task<JsonResult> UpdateUserAddress(Address address) {
            var errors = address.VerifyAddressData();
            if (errors.Count != 0) {
                var errorMessages = address.GenerateErrorMessages(errors);
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errorMessages });
            }

            var result = await _addressService.UpdateAddress(address);
            return result.HasValue && result.Value
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
        }

        [HttpDelete("remove-address/{addressId}")]
        [RoutinizeActionFilter]
        public async Task<JsonResult> RemoveAddress(int addressId) {
            var result = await _addressService.RemoveAddress(addressId);
            return result.HasValue && result.Value
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while removing data." });
        }

        [HttpPut("update-user-privacy/{accountId}")]
        [RoutinizeActionFilter]
        public async Task<JsonResult> UpdateUserPrivacy([FromRoute] int accountId,[FromBody] UserPrivacy userPrivacy) {
            if (userPrivacy.UserId < 1) {
                var saveProfileResult = await _userService.InsertBlankUserWithPrivacyAndAppSetting(accountId);
                if (!saveProfileResult.HasValue || saveProfileResult < 1)
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });

                userPrivacy.UserId = saveProfileResult.Value;
            }

            var updatePrivacyResult = await _userService.UpdateUserPrivacy(userPrivacy);
            return updatePrivacyResult.HasValue && updatePrivacyResult.Value
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = userPrivacy.UserId })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
        }

        [HttpPut("update-app-settings/{accountId}")]
        [RoutinizeActionFilter]
        public async Task<JsonResult> UpdateAppSettings([FromRoute] int accountId,[FromBody] AppSetting appSetting) {
            if (appSetting.UserId < 1) {
                var saveProfileResult = await _userService.InsertBlankUserWithPrivacyAndAppSetting(accountId);
                if (!saveProfileResult.HasValue || saveProfileResult < 1)
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });

                appSetting.UserId = saveProfileResult.Value;
            }

            var updateSettingResult = await _userService.UpdateUserAppSettings(appSetting);
            return updateSettingResult.HasValue && updateSettingResult.Value
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = appSetting.UserId })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
        }

        [HttpGet("get-user-privacy/{accountId}")]
        [RoutinizeActionFilter]
        public async Task<JsonResult> GetUserPrivacy(int accountId) {
            var profileExisted = await _userService.IsUserProfileCreated(accountId);
            if (!profileExisted.HasValue || !profileExisted.Value)
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            var (error, userPrivacy) = await _userService.GetUserPrivacy(accountId);
            if (error) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            userPrivacy ??= new UserPrivacy();
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = userPrivacy });
        }

        [HttpGet("get-app-settings/{accountId}")]
        [RoutinizeActionFilter]
        public async Task<JsonResult> GetUserAppSettings(int accountId) {
            var profileExisted = await _userService.IsUserProfileCreated(accountId);
            if (!profileExisted.HasValue || !profileExisted.Value)
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            var (error, appSetting) = await _userService.GetAppSettings(accountId);
            if (error) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            appSetting ??= new AppSetting();
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = appSetting });
        }
    }
}