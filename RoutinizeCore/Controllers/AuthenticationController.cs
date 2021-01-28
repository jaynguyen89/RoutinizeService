using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AssistantLibrary.Interfaces;
using AssistantLibrary.Models;
using HelperLibrary;
using HelperLibrary.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RoutinizeCore.Attributes;
using RoutinizeCore.Models;
using RoutinizeCore.Services.ApplicationServices.CacheService;
using RoutinizeCore.Services.Interfaces;
using RoutinizeCore.ViewModels;
using RoutinizeCore.ViewModels.AccountRecovery;
using RoutinizeCore.ViewModels.Authentication;

namespace RoutinizeCore.Controllers {

    [ApiController]
    [Route("authentication")]
    public sealed class AuthenticationController : ControllerBase  {
        
        private readonly IAuthenticationService _authenticationService;
        private readonly IAccountService _accountService;
        private readonly IUserService _userService;
        private readonly IChallengeService _challengeService;

        private readonly IAssistantService _assistantService;
        private readonly IGoogleRecaptchaService _googleRecaptchaService;
        private readonly IEmailSenderService _emailSenderService;

        private AuthSettings _authSettings = new AuthSettings();

        private sealed class AuthSettings {
            public int AccessFailedAttempts { get; set; }
            public int LockoutDuration { get; set; }
        }

        public AuthenticationController(
            IAuthenticationService authenticationService,
            IAccountService accountService,
            IUserService userService,
            IChallengeService challengeService,
            IAssistantService assistantService,
            IGoogleRecaptchaService googleRecaptchaService,
            IEmailSenderService emailSenderService,
            IOptions<ApplicationOptions> appOptions
        ) {
            _authenticationService = authenticationService;
            _accountService = accountService;
            _userService = userService;
            _challengeService = challengeService;
            _assistantService = assistantService;
            _googleRecaptchaService = googleRecaptchaService;
            _emailSenderService = emailSenderService;

            _authSettings.AccessFailedAttempts = int.Parse(appOptions.Value.AccessFailedAttempts);
            _authSettings.LockoutDuration = int.Parse(appOptions.Value.LockoutDuration);
        }

        public static JsonResult UnauthenticationResult(SharedEnums.ActionFilterResults actionFilterResults) {
            return actionFilterResults switch {
                SharedEnums.ActionFilterResults.UnauthenticatedRequest => new JsonResult(new JsonResponse {
                    Result = SharedEnums.RequestResults.Denied,
                    Message = "Error: Your access to resource is denied.",
                    Error = SharedEnums.HttpStatusCodes.Forbidden
                }),
                SharedEnums.ActionFilterResults.RequestProcessingError => new JsonResult(new JsonResponse {
                    Result = SharedEnums.RequestResults.Denied,
                    Message = "Error: Improper request format.",
                    Error = SharedEnums.HttpStatusCodes.Forbidden
                }),
                _ => throw new ArgumentOutOfRangeException(nameof(actionFilterResults), actionFilterResults, null)
            };
        }

        [HttpGet("check-registration-email-availability/{email}")]
        public async Task<JsonResult> CheckRegistrationEmailAvailability(string email) {
            if (!Helpers.IsProperString(email))
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "Missing data." });
            
            email = email.Trim().ToLower();
            var emailAvailable = await _accountService.IsRegistrationEmailAvailable(email);
            
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = new { emailAvailable } });
        }

        [HttpGet("check-username-availability/{username}")]
        public async Task<JsonResult> CheckUsernameAvailability(string username) {
            if (!Helpers.IsProperString(username))
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "Missing data." });

            username = username.Trim().ToLower();
            var usernameAvailable = await _accountService.IsUsernameAvailable(username);
            
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = new { usernameAvailable } });
        }

        [HttpPost("register-account")]
        public async Task<JsonResult> RegisterAccount(RegisterAccountVM registrationData) {
            //var isRequestedByHuman = await _googleRecaptchaService.IsHumanRegistration(registrationData.RecaptchaToken);
            //if (!isRequestedByHuman.Result) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Error = SharedEnums.HttpStatusCodes.ImATeapot });

            var userInputsVerification = VerifyRegistrationData(registrationData);
            if (userInputsVerification.Count != 0) {
                var errorMessages = registrationData.GenerateErrorMessages(userInputsVerification);
                return new JsonResult(new JsonResponse {
                    Result = SharedEnums.RequestResults.Failed, Error = SharedEnums.HttpStatusCodes.Conflict, Data = errorMessages
                });
            }

            var isEmailAvailable = await _accountService.IsRegistrationEmailAvailable(registrationData.Email);
            var isUsernameAvailable = await _accountService.IsUsernameAvailable(registrationData.Username);

            if (!isEmailAvailable || !isUsernameAvailable)
                return new JsonResult(new JsonResponse {
                    Result = SharedEnums.RequestResults.Failed, Error = SharedEnums.HttpStatusCodes.Conflict,
                    Message = "Email or Username has been registered for other account(s)."
                });

            var (hashedPassword, salt) = _assistantService.GenerateHashAndSalt(registrationData.Password);
            registrationData.Password = hashedPassword;
            registrationData.PasswordConfirm = salt;

            var isAccountUniqueIdValid = false;
            var accountUniqueId = string.Empty;
            while (!isAccountUniqueIdValid) {
                accountUniqueId = Helpers.GenerateRandomString(SharedConstants.ACCOUNT_UNIQUE_ID_LENGTH);
                isAccountUniqueIdValid = await _accountService.IsAccountUniqueIdAvailable(accountUniqueId);

                if (isAccountUniqueIdValid)
                    accountUniqueId = accountUniqueId.ToUpper(); //Helpers.AppendCharacterToString(accountUniqueId);
            }

            var activationToken = Helpers.GenerateRandomString(SharedConstants.ACCOUNT_ACTIVATION_TOKEN_LENGTH);
            var newAccountId = await _authenticationService.InsertNewUserAccount(registrationData, accountUniqueId, activationToken);

            if (newAccountId < 1) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed });

            if (await SendAccountActivationEmail(registrationData.Username, registrationData.Email, activationToken))
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });

            await _authenticationService.RemoveNewlyInsertedUserAccount(newAccountId);
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed });
        }

        private List<int> VerifyRegistrationData(RegisterAccountVM registrationData) {
            var errors = registrationData.VerifyEmail();
            errors.AddRange(registrationData.VerifyUserName());
            errors.AddRange(registrationData.VerifyPassword());

            return errors;
        }

        [HttpPost("activate-account")]
        public async Task<JsonResult> ActivateAccount(AccountActivationVM activator) {
            //var isRequestedByHuman = await _googleRecaptchaService.IsHumanRegistration(activator.RecaptchaToken);
            //if (!isRequestedByHuman.Result) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Error = SharedEnums.HttpStatusCodes.ImATeapot });

            var (activationResult, isPositiveResult) = await _authenticationService.ActivateUserAccount(activator);
            if (!activationResult) return new JsonResult(new JsonResponse {
                Result = SharedEnums.RequestResults.Failed,
                Message = isPositiveResult == null ? "No account matched the activation data in our record." : (
                        !isPositiveResult.Value
                            ? "We encountered an internal error while activating your account."
                            : "The activation data have expired. Please request another activation email."
                    ),
                Error = isPositiveResult == null ? SharedEnums.HttpStatusCodes.NotFound : (
                        !isPositiveResult.Value ? SharedEnums.HttpStatusCodes.InternalServerError : SharedEnums.HttpStatusCodes.RequestTimeout    
                    )
            });

            if (!isPositiveResult.HasValue || !isPositiveResult.Value)
                return new JsonResult(new JsonResponse {
                    Result = SharedEnums.RequestResults.Failed,
                    Message = "We encountered an internal error while activating your account."
                });

            using var fileReader = System.IO.File.OpenText($"{ SharedConstants.EMAIL_TEMPLATES_DIRECTORY }AccountActivationConfirmationEmail.html");
            var accountActivationConfirmationEmailTemplate = await fileReader.ReadToEndAsync();

            var userAccount = await _accountService.GetUserAccountByEmail(activator.Email);
            var accountActivationConfirmationEmailContent = accountActivationConfirmationEmailTemplate.Replace(
                "[USER_NAME]", userAccount != null ? userAccount.Username : activator.Email
            );

            var accountActivationConfirmationEmail = new EmailContent {
                Subject = "Account activated",
                Body = accountActivationConfirmationEmailContent,
                ReceiverName = userAccount != null ? userAccount.Username : "Routinize User",
                ReceiverAddress = activator.Email
            };
            
            if (userAccount != null) await _userService.InsertBlankUserOnAccountRegistration(userAccount.Id);
            
            if (await _emailSenderService.SendEmailSingle(accountActivationConfirmationEmail))
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });

            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Partial, Message = "Confirmation email was failed to send."});
        }

        [HttpPost("request-new-account-activation-email")]
        public async Task<JsonResult> SendNewAccountActivationEmail(AccountActivationVM activator) {
            //var isRequestedByHuman = await _googleRecaptchaService.IsHumanRegistration(activator.RecaptchaToken);
            //if (!isRequestedByHuman.Result) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Error = SharedEnums.HttpStatusCodes.ImATeapot });

            var userAccount = await _accountService.GetUserAccountByEmail(activator.Email);
            if (userAccount == null) return new JsonResult(new JsonResponse {
                Result = SharedEnums.RequestResults.Failed,
                Message = "No account matched the requested email in our record.",
                Error = SharedEnums.HttpStatusCodes.NotFound
            });
            
            userAccount.RecoveryToken = Helpers.GenerateRandomString(SharedConstants.ACCOUNT_ACTIVATION_TOKEN_LENGTH);
            userAccount.TokenSetOn = DateTime.UtcNow;

            if (await _accountService.UpdateUserAccount(userAccount) &&
                await SendAccountActivationEmail(userAccount.Username, userAccount.Email, userAccount.RecoveryToken)
            )
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });

            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Error = SharedEnums.HttpStatusCodes.InternalServerError });
        }

        private async Task<bool> SendAccountActivationEmail(string username, string email, string activationToken) {
            using var fileReader = System.IO.File.OpenText($"{ SharedConstants.EMAIL_TEMPLATES_DIRECTORY }AccountActivationEmail.html");
            var accountActivationEmailContent = await fileReader.ReadToEndAsync();

            accountActivationEmailContent = accountActivationEmailContent.Replace("[USER_NAME]", username);
            accountActivationEmailContent = accountActivationEmailContent.Replace("[USER_EMAIL]", email);
            accountActivationEmailContent = accountActivationEmailContent.Replace("[ACTIVATION_TOKEN]", activationToken);
            accountActivationEmailContent = accountActivationEmailContent.Replace("[VALIDITY_DURATION]", SharedConstants.ACCOUNT_ACTIVATION_EMAIL_VALIDITY_DURATION.ToString());

            var accountActivationEmail = new EmailContent {
                Subject = "Activate your account",
                Body = accountActivationEmailContent,
                ReceiverName = username,
                ReceiverAddress = email
            };

            fileReader.Close();
            return await _emailSenderService.SendEmailSingle(accountActivationEmail);
        }

        [HttpPost("authenticate")]
        public async Task<JsonResult> Authenticate(AuthenticationVM authenticationData) {
            //var isRequestedByHuman = await _googleRecaptchaService.IsHumanRegistration(authenticationData.RecaptchaToken);
            //if (!isRequestedByHuman.Result) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Error = SharedEnums.HttpStatusCodes.ImATeapot });

            var authenticationDataVerification = authenticationData.VerifyAuthenticationData();
            if (authenticationDataVerification.Count != 0) {
                var errorMessages = authenticationData.GenerateErrorMessages(authenticationDataVerification);
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errorMessages });
            }

            var (isSuccessful, userAccount) = await _authenticationService.AuthenticateUserAccount(authenticationData);
            if (!isSuccessful || userAccount == null)
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "That credentials is not a match." });

            if (userAccount.LockoutEnd.HasValue && userAccount.LockoutEnd >= DateTime.UtcNow)
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Denied, Message = "Your account is locked." });

            if (!_assistantService.IsHashMatchesPlainText(userAccount.PasswordHash, authenticationData.Password)) {
                userAccount.AccessFailedCount += 1;
                var loginAttemptsLeft = _authSettings.AccessFailedAttempts - userAccount.AccessFailedCount % _authSettings.AccessFailedAttempts;
                userAccount.LockoutEnabled = loginAttemptsLeft == _authSettings.AccessFailedAttempts;
                userAccount.LockoutEnd = userAccount.LockoutEnabled ? DateTime.UtcNow.AddMinutes(_authSettings.LockoutDuration) : null;

                _ = await _accountService.UpdateUserAccount(userAccount);
                return new JsonResult(new JsonResponse {
                    Result = SharedEnums.RequestResults.Failed,
                    Message = userAccount.LockoutEnabled
                        ? $"Your account is locked. Please try again in { _authSettings.LockoutDuration } minutes."
                        : $"Incorrect login credentials. You have { loginAttemptsLeft } { (loginAttemptsLeft > 1 ? "attempts" : "attempt") } left."
                });
            }

            if (userAccount.LockoutEnabled) {
                userAccount.AccessFailedCount = 0;
                userAccount.LockoutEnabled = false;
                userAccount.LockoutEnd = null;
                
                var updateResult = await _accountService.UpdateUserAccount(userAccount);
                if (!updateResult) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
            }

            var authenticationTimestamp = DateTime.UtcNow;
            var tokenSalt = _assistantService.GenerateSaltForHash();
            var authenticationToken = Helpers.GenerateSha512Hash(
                $"{ userAccount.Id }{ Helpers.ConvertToUnixTimestamp(authenticationTimestamp) }{ tokenSalt }"
            );

            var (authenticatedUser, authenticationRecord) = CreateAuthenticatedUserAndRecord(userAccount, authenticationData.TrustedAuth, authenticationData.DeviceInformation);

            var authSavingResult = await _authenticationService.InsertAuthenticationRecord(authenticationRecord);
            if (authSavingResult.HasValue && authSavingResult.Value)
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = authenticatedUser });
            
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "We encountered an issue while establishing your session." });
        }

        [HttpPost("authenticate-session")]
        public async Task<JsonResult> AuthenticateSession(SessionAuthVM sessionAuthData) {
            var authRecord = await _authenticationService.GetLatestAuthRecordForUserAccount(sessionAuthData.AccountId);
            if (authRecord == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed });

            var dbAccount = await _authenticationService.GetAccountById(authRecord.AccountId);
            if (dbAccount == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed });
            
            var sessionAuthExpiryTimestamp = authRecord.AuthTimestamp + (
                authRecord.TrustedAuth ?
                    SharedConstants.TRUSTED_AUTHENTICATION_EXPIRY_DURATION :
                    SharedConstants.UNTRUSTED_AUTHENTICATION_EXPIRY_DURATION
            ) * SharedConstants.MINUTES_PER_HOUR * SharedConstants.SECONDS_PER_MINUTE * SharedConstants.MILLIS_PER_SECOND;
            if (sessionAuthExpiryTimestamp < Helpers.ConvertToUnixTimestamp(DateTime.UtcNow))
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed });
            
            var reliableAuthToken = Helpers.GenerateSha512Hash(
                $"{ authRecord.AccountId }{ authRecord.AuthTimestamp }{ authRecord.AuthTokenSalt }"
            );

            if (!reliableAuthToken.Equals(sessionAuthData.AuthToken))
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed });
            
            var (authenticatedUser, authenticationRecord) = CreateAuthenticatedUserAndRecord(dbAccount, sessionAuthData.GetTrustedAuth(), sessionAuthData.DeviceInformation);

            var authSavingResult = await _authenticationService.InsertAuthenticationRecord(authenticationRecord);
            if (authSavingResult.HasValue && authSavingResult.Value)
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = authenticatedUser });
                
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "We encountered an issue while establishing your session." });
        }

        private KeyValuePair<AuthenticatedUser, AuthRecord> CreateAuthenticatedUserAndRecord(
            Account userAccount, bool trustedAuth, string deviceInformation
        ) {
            var authenticationTimestamp = DateTime.UtcNow;
            var tokenSalt = _assistantService.GenerateSaltForHash();
            var authenticationToken = Helpers.GenerateSha512Hash(
                $"{ userAccount.Id }{ Helpers.ConvertToUnixTimestamp(authenticationTimestamp) }{ tokenSalt }"
            );
            
            var authenticatedUser = new AuthenticatedUser {
                AccountId = userAccount.Id,
                AuthToken = authenticationToken
            };
            authenticatedUser.SetTrustedAuth(trustedAuth);
            
            // _memoryCache.SetCacheEntry<AuthenticatedUser>(new CacheEntry {
            //     EntryKey = $"{ nameof(AuthenticatedUser) }_{ userAccount.Id }",
            //     Priority = CacheItemPriority.High,
            //     Size = authenticatedUser.GetType().GetProperties().Length,
            //     Data = authenticatedUser
            // });
            
            var authenticationRecord = new AuthRecord {
                AccountId = userAccount.Id,
                AuthTokenSalt = tokenSalt,
                AuthTimestamp = Helpers.ConvertToUnixTimestamp(authenticationTimestamp),
                DeviceInformation = deviceInformation,
                TrustedAuth = trustedAuth
            };

            return new KeyValuePair<AuthenticatedUser, AuthRecord>(authenticatedUser, authenticationRecord);
        }

        [HttpGet("sign-out/{accountId}")]
        [RoutinizeActionFilter]
        public async Task<JsonResult> Unauthenticate(int accountId) {
            await _authenticationService.RevokeAuthRecord(accountId);
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }

        [HttpPost("forgot-password")]
        public async Task<JsonResult> ForgotPassword(ForgotPasswordVM forgotPasswordData) {
            var isRequestedByHuman = await _googleRecaptchaService.IsHumanRegistration(forgotPasswordData.RecaptchaToken);
            if (!isRequestedByHuman.Result) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Error = SharedEnums.HttpStatusCodes.ImATeapot });

            var error = forgotPasswordData.VerifyForgotPasswordData();
            if (error != null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = error });

            var userAccount = await _accountService.GetUserAccountByEmail(forgotPasswordData.Email);
            if (userAccount == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while searching for your account." });

            userAccount.EmailConfirmed = false;
            userAccount.RecoveryToken = Helpers.GenerateRandomString(SharedConstants.ACCOUNT_ACTIVATION_TOKEN_LENGTH);
            userAccount.TokenSetOn = DateTime.UtcNow;

            if (!await _accountService.UpdateUserAccount(userAccount))
                return new JsonResult(new JsonResponse {
                    Result = SharedEnums.RequestResults.Failed,
                    Message = "An issue happened while attempting to recover your account.",
                    Error = SharedEnums.HttpStatusCodes.InternalServerError
                });
            
            var emailSendingResult = await SendPasswordResetEmail(userAccount.Username, userAccount.Email, userAccount.Id, userAccount.RecoveryToken);
            return emailSendingResult
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Partial, Message = "Failed to send Recover Password email." });
        }

        [HttpGet("request-new-password-reset-email")]
        public async Task<JsonResult> SendNewPasswordResetEmail(ForgotPasswordVM forgotPasswordData) {
            var isRequestedByHuman = await _googleRecaptchaService.IsHumanRegistration(forgotPasswordData.RecaptchaToken);
            if (!isRequestedByHuman.Result) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Error = SharedEnums.HttpStatusCodes.ImATeapot });

            var error = forgotPasswordData.VerifyForgotPasswordData();
            if (error != null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = error });
            
            var userAccount = await _accountService.GetUserAccountById(forgotPasswordData.AccountId, false);
            if (userAccount == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while searching for your account." });
            
            userAccount.RecoveryToken = Helpers.GenerateRandomString(SharedConstants.ACCOUNT_ACTIVATION_TOKEN_LENGTH);
            userAccount.TokenSetOn = DateTime.UtcNow;
            
            if (!await _accountService.UpdateUserAccount(userAccount))
                return new JsonResult(new JsonResponse {
                    Result = SharedEnums.RequestResults.Failed,
                    Message = "An issue happened while attempting to recover your account.",
                    Error = SharedEnums.HttpStatusCodes.InternalServerError
                });
            
            var emailSendingResult = await SendPasswordResetEmail(userAccount.Username, userAccount.Email, userAccount.Id, userAccount.RecoveryToken);
            return emailSendingResult
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Partial, Message = "Failed to send Recover Password email." });
        }

        [HttpPost("reset-password")]
        public async Task<JsonResult> RecoverPassword(PasswordRecoveryVM passwordRecoveryData) {
            var dataErrors = passwordRecoveryData.VerifyNewPassword();
            if (dataErrors.Count != 0) {
                var errorMessages = passwordRecoveryData.GenerateErrorMessages(dataErrors);
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errorMessages });
            }

            var challengeVerification = await _challengeService.VerifyChallengeProofFor(passwordRecoveryData.AccountId, passwordRecoveryData.ChallengeResponse);
            if (!challengeVerification.HasValue)
                return new JsonResult(new JsonResponse {
                    Result = SharedEnums.RequestResults.Failed,
                    Message = "An issue happened while processing your new password.",
                    Error = SharedEnums.HttpStatusCodes.InternalServerError
                });
            
            if (!challengeVerification.Value) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "Challenge response failed." });

            var userAccount = await _accountService.GetUserAccountById(passwordRecoveryData.AccountId, false);
            if (userAccount == null)
                return new JsonResult(new JsonResponse {
                    Result = SharedEnums.RequestResults.Failed,
                    Message = "An issue happened while processing your new password.",
                    Error = SharedEnums.HttpStatusCodes.InternalServerError
                });
            
            var (hashedPassword, salt) = _assistantService.GenerateHashAndSalt(passwordRecoveryData.NewPassword);
            userAccount.PasswordHash = hashedPassword;
            userAccount.PasswordSalt = salt;
            userAccount.EmailConfirmed = true;
            userAccount.RecoveryToken = null;
            userAccount.TokenSetOn = null;

            var result = await _accountService.UpdateUserAccount(userAccount);
            return result
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success })
                : new JsonResult(new JsonResponse {
                    Result = SharedEnums.RequestResults.Failed,
                    Message = "An issue happened while processing your new password.",
                    Error = SharedEnums.HttpStatusCodes.InternalServerError
                });
        }
        
        private async Task<bool> SendPasswordResetEmail(string username, string email, int accountId, string activationToken) {
            using var fileReader = System.IO.File.OpenText($"{ SharedConstants.EMAIL_TEMPLATES_DIRECTORY }RecoverPasswordEmail.html");
            var recoverPasswordEmailContent = await fileReader.ReadToEndAsync();

            recoverPasswordEmailContent = recoverPasswordEmailContent.Replace("[USER_NAME]", username);
            recoverPasswordEmailContent = recoverPasswordEmailContent.Replace("[ACCOUNT_ID]", accountId.ToString());
            recoverPasswordEmailContent = recoverPasswordEmailContent.Replace("[ACTIVATION_TOKEN]", activationToken);
            recoverPasswordEmailContent = recoverPasswordEmailContent.Replace("[VALIDITY_DURATION]", SharedConstants.ACCOUNT_ACTIVATION_EMAIL_VALIDITY_DURATION.ToString());

            var recoverPasswordEmail = new EmailContent {
                Subject = "Activate your account",
                Body = recoverPasswordEmailContent,
                ReceiverName = username,
                ReceiverAddress = email
            };

            fileReader.Close();
            return await _emailSenderService.SendEmailSingle(recoverPasswordEmail);
        }
    }
}