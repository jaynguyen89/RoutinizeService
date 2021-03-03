using System.Threading.Tasks;
using AssistantLibrary.Interfaces;
using HelperLibrary;
using HelperLibrary.Shared;
using MediaLibrary.Interfaces;
using MediaLibrary.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RoutinizeCore.Attributes;
using RoutinizeCore.Models;
using RoutinizeCore.Services.Interfaces;
using RoutinizeCore.ViewModels;
using RoutinizeCore.ViewModels.User;

namespace RoutinizeCore.Controllers {

    [ApiController]
    [Route("user")]
    [RoutinizeActionFilter]
    public sealed class UserController {

        private readonly IUserService _userService;
        private readonly IAddressService _addressService;
        private readonly IAvatarService _avatarService;
        private readonly IRsaService _rsaService;

        public UserController(
            IUserService userService,
            IAddressService addressService,
            IAvatarService avatarService,
            IRsaService rsaService
        ) {
            _userService = userService;
            _addressService = addressService;
            _avatarService = avatarService;
            _rsaService = rsaService;
        }

        [HttpGet("is-user-profile-initialized")]
        public async Task<JsonResult> IsUserProfileInitialized([FromHeader] int accountId) {
            var result = await _userService.CheckIfUserProfileInitialized(accountId);
            return !result.HasValue ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." })
                                    : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = result.Value });
        }

        [HttpGet("initialize-user-profile")]
        public async Task<JsonResult> InitializeUserProfile([FromHeader] int accountId) {
            var result = await _userService.InsertBlankUserWithPrivacyAndAppSetting(accountId);
            return !result.HasValue || result.Value < 1
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = result.Value });
        }

        [HttpGet("profile")]
        public async Task<JsonResult> GetUserProfile([FromHeader] int accountId) {
            var (error, userProfile) = await _userService.GetUserProfileByAccountId(accountId);
            if (error) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            userProfile ??= new User { AccountId = accountId };
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = userProfile });
        }

        [HttpPost("set-profile-avatar")]
        public async Task<JsonResult> SetProfileAvatar([FromHeader] int accountId,[FromBody] ProfileAvatarVM avatarData) {
            var errors = avatarData.CheckAvatar();
            if (errors.Count != 0) {
                var errorMessages = avatarData.GenerateErrorMessages(errors);
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errorMessages });
            }

            var (error, userProfile) = await _userService.GetUserProfileByAccountId(accountId);
            if (error) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            bool result;
            if (Helpers.IsProperString(avatarData.AvatarName)) {
                result = await SaveUserProfileData(userProfile, accountId, avatarData.AvatarName);
                return result
                    ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." })
                    : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
            }

            if (avatarData.AvatarFile == null)
                return new JsonResult(new JsonResponse {Result = SharedEnums.RequestResults.Failed, Error = SharedEnums.HttpStatusCodes.NoContent});
            
            var httpResult = await _avatarService.SendSaveAvatarRequestToRoutinizeStorageApi(
                new ImgUploadVM {
                    AccountId = accountId,
                    TokenId = avatarData.TokenId,
                    UploadedFile = avatarData.AvatarFile
                });

            if (httpResult.Error) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
                
            result = await SaveUserProfileData(userProfile, accountId, httpResult.Result.Name, httpResult.Result.Location);
            return result
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }

        [HttpPut("update-profile-avatar")]
        public async Task<JsonResult> UpdateProfileAvatar([FromHeader] int accountId,[FromBody] ProfileAvatarVM avatarData) {
            var errors = avatarData.CheckAvatar();
            if (errors.Count != 0) {
                var errorMessages = avatarData.GenerateErrorMessages(errors);
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errorMessages });
            }
            
            var (error, userProfile) = await _userService.GetUserProfileByAccountId(accountId);
            if (error || userProfile == null)
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            bool result;
            if (Helpers.IsProperString(avatarData.AvatarName)) {
                result = await SaveUserProfileData(userProfile, accountId, avatarData.AvatarName);
                return result
                    ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." })
                    : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
            }
            
            if (avatarData.AvatarFile == null)
                return new JsonResult(new JsonResponse {Result = SharedEnums.RequestResults.Failed, Error = SharedEnums.HttpStatusCodes.NoContent});
            
            var httpResult = await _avatarService.SendReplaceAvatarRequestToRoutinizeStorageApi(
                new ImgReplaceVM {
                    AccountId = accountId,
                    CurrentImage = JsonConvert.DeserializeObject<AvatarVM>(userProfile.AvatarName).Name,
                    TokenId = avatarData.TokenId,
                    FileToSave = avatarData.AvatarFile
                });
            
            if (httpResult.Error) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
            
            result = await SaveUserProfileData(userProfile, accountId, httpResult.Result.Name, httpResult.Result.Location);
            return result
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }
        
        [HttpDelete("remove-profile-avatar/{tokenId}")]
        public async Task<JsonResult> RemoveProfileAvatar([FromHeader] int accountId,[FromRoute] int tokenId) {
            var (error, userProfile) = await _userService.GetUserProfileByAccountId(accountId);
            if (error || userProfile == null)
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            var avatarName = userProfile.AvatarName;
            userProfile.AvatarName = null;
            var result = await _userService.UpdateUserProfile(userProfile);
            if (!result.HasValue || !result.Value)
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });

            if (!avatarName.Contains("/files/")) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
            var httpResult = await _avatarService.SendDeleteAvatarRequestToRoutinizeStorageApi(
                tokenId, accountId,
                JsonConvert.DeserializeObject<AvatarVM>(avatarName).Name
            );
            
            return httpResult.Error
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
            
        }

        private async Task<bool> SaveUserProfileData(User profile, int accountId, string avatarName, string avatarLocation = null) {
            bool? result;
            
            if (profile == null) {
                profile = new User {
                    AccountId = accountId,
                    AvatarName = avatarName
                };

                var saveResult = await _userService.SaveNewUserProfile(profile);
                result = saveResult > 0;
            }
            else {
                profile.AvatarName = JsonConvert.SerializeObject(new AvatarVM { Location = avatarLocation, Name = avatarName });
                result = await _userService.UpdateUserProfile(profile);
            }

            return result.HasValue && result.Value;
        }

        [HttpPost("update-profile")]
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

        [HttpGet("signature")]
        public async Task<JsonResult> DoesUserHaveSignature([FromHeader] int userId) {
            var (error, keyDate) = await _userService.CheckActiveRsaKey(userId);
            return error ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." })
                         : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = keyDate });
        }

        [HttpPut("new-signature")]
        public async Task<JsonResult> GenerateNewRsaKey([FromHeader] int userId) {
            _rsaService.GenerateRsaKeyPair();
            var saveResult = await _userService.SaveNewUserRsaKey(userId, _rsaService.PublicKey, _rsaService.PrivateKey);
            return !saveResult.HasValue || !saveResult.Value
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }

        [HttpPost("save-user-address")]
        public async Task<JsonResult> SaveUserAddress([FromHeader] int accountId, [FromBody] Address address) {
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

            await _userService.StartTransaction();
            
            var saveAddressResult = await _addressService.SaveNewAddress(address);
            if (!saveAddressResult.HasValue || saveAddressResult.Value < 1) {
                await _userService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
            }

            userProfile.AddressId = saveAddressResult;
            var updateResult = await _userService.UpdateUserProfile(userProfile);
            if (updateResult.HasValue && updateResult.Value) {
                await _userService.CommitTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = userProfile });
            }

            await _userService.RevertTransaction();
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
        }

        //Todo: Consider to remove this method, use AddressController.UpdateAddress instead
        [HttpPut("update-user-address")]
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
        public async Task<JsonResult> RemoveAddress([FromHeader] int accountId,[FromRoute] int addressId) {
            var (error, userProfile) = await _userService.GetUserProfileByAccountId(accountId);
            if (error || userProfile == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            userProfile.AddressId = null;
            await _userService.StartTransaction();

            var updateUserResult = await _userService.UpdateUserProfile(userProfile);
            if (!updateUserResult.HasValue || !updateUserResult.Value) {
                await _userService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
            }
            
            var result = await _addressService.RemoveAddress(addressId);
            if (result.HasValue && result.Value) {
                await _userService.CommitTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
            }

            await _userService.RevertTransaction();
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while removing data." });
        }

        [HttpPut("update-user-privacy")]
        public async Task<JsonResult> UpdateUserPrivacy([FromHeader] int accountId,[FromBody] UserPrivacy userPrivacy) {
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

        [HttpPut("update-app-settings")]
        public async Task<JsonResult> UpdateAppSettings([FromHeader] int accountId,[FromBody] AppSetting appSetting) {
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

        [HttpGet("get-user-privacy")]
        public async Task<JsonResult> GetUserPrivacy([FromHeader] int accountId) {
            var profileExisted = await _userService.IsUserProfileCreated(accountId);
            if (!profileExisted.HasValue || !profileExisted.Value)
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            var (error, userPrivacy) = await _userService.GetUserPrivacy(accountId);
            if (error) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            userPrivacy ??= new UserPrivacy();
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = userPrivacy });
        }

        [HttpGet("get-app-settings")]
        public async Task<JsonResult> GetUserAppSettings([FromHeader] int accountId) {
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