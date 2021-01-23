using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AssistantLibrary.Interfaces;
using AssistantLibrary.Models;
using HelperLibrary;
using HelperLibrary.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoLibrary.Interfaces;
using Newtonsoft.Json;
using RoutinizeCore.Models;
using RoutinizeCore.Services.Interfaces;
using RoutinizeCore.ViewModels;
using RoutinizeCore.ViewModels.Authentication;

namespace RoutinizeCore.Controllers {

    [ApiController]
    [Route("authentication")]
    public sealed class AuthenticationController : ControllerBase  {
        
        private readonly IRoutinizeCoreLogService _coreLogService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IAccountService _accountService;
        private readonly IUserService _userService;

        private readonly IAssistantService _assistantService;
        private readonly IGoogleRecaptchaService _googleRecaptchaService;
        private readonly IEmailSenderService _emailSenderService;

        public AuthenticationController(
            IRoutinizeCoreLogService coreLogService,
            IAuthenticationService authenticationService,
            IAccountService accountService,
            IUserService userService,
            IAssistantService assistantService,
            IGoogleRecaptchaService googleRecaptchaService,
            IEmailSenderService emailSenderService
        ) {
            _coreLogService = coreLogService;
            _authenticationService = authenticationService;
            _accountService = accountService;
            _userService = userService;
            _assistantService = assistantService;
            _googleRecaptchaService = googleRecaptchaService;
            _emailSenderService = emailSenderService;
        }

        public static JsonResult UnauthenticationResult(SharedEnums.ActionFilterResults actionFilterResults) {
            return actionFilterResults switch {
                SharedEnums.ActionFilterResults.UnauthenticatedRequest => new JsonResult(new JsonResponse {
                    Result = SharedEnums.RequestResults.Denied,
                    Message = "Error: Your access to resource is denied.",
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
            
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = emailAvailable });
        }

        [HttpGet("check-username-availability/{username}")]
        public async Task<JsonResult> CheckUsernameAvailability(string username) {
            if (!Helpers.IsProperString(username))
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "Missing data." });

            username = username.Trim().ToLower();
            var usernameAvailable = await _accountService.IsUsernameAvailable(username);
            
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = usernameAvailable });
        }

        [HttpPost("register-account")]
        public async Task<JsonResult> RegisterAccount(RegisterAccountVM registrationData) {
            var isRequestedByHuman = await _googleRecaptchaService.IsHumanRegistration(registrationData.RecaptchaToken);
            if (!isRequestedByHuman.Result) return new JsonResult(isRequestedByHuman);

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
                accountUniqueId = _assistantService.GenerateRandomString(SharedConstants.ACCOUNT_UNIQUE_ID_LENGTH);
                isAccountUniqueIdValid = await _accountService.IsAccountUniqueIdAvailable(accountUniqueId);

                if (isAccountUniqueIdValid)
                    accountUniqueId = Helpers.AppendCharacterToString(accountUniqueId);
            }

            var activationToken = _assistantService.GenerateRandomString(SharedConstants.ACCOUNT_ACTIVATION_TOKEN_LENGTH);
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
            var isRequestedByHuman = await _googleRecaptchaService.IsHumanRegistration(activator.RecaptchaToken);
            if (!isRequestedByHuman.Result) return new JsonResult(isRequestedByHuman);

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

            var userAccount = await _accountService.GetUnactivatedUserAccountByEmail(activator.Email);
            var accountActivationConfirmationEmailContent = accountActivationConfirmationEmailTemplate.Replace(
                "[USER_NAME]", userAccount != null ? userAccount.Username : activator.Email
            );

            var accountActivationConfirmationEmail = new EmailContent {
                Subject = "Account activated",
                Body = accountActivationConfirmationEmailContent,
                ReceiverName = userAccount != null ? userAccount.Username : "Routinize User",
                ReceiverAddress = activator.Email
            };
            
            if (userAccount != null) await _userService.InsertBlankUserOnAccountRegistration(userAccount.Id); //TODO: later on signin, check if user account is created, if not, attempt to create it
            
            if (await _emailSenderService.SendEmailSingle(accountActivationConfirmationEmail))
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });

            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Partial, Message = "Confirmation email was failed to send."});
        }

        [HttpPost("request-new-account-activation-email")]
        public async Task<JsonResult> SendNewAccountActivationEmail(AccountActivationVM activator) {
            var isRequestedByHuman = await _googleRecaptchaService.IsHumanRegistration(activator.RecaptchaToken);
            if (!isRequestedByHuman.Result) return new JsonResult(isRequestedByHuman);

            var userAccount = await _accountService.GetUnactivatedUserAccountByEmail(activator.Email);
            if (userAccount == null) return new JsonResult(new JsonResponse {
                Result = SharedEnums.RequestResults.Failed,
                Message = "No account matched the requested email in our record.",
                Error = SharedEnums.HttpStatusCodes.NotFound
            });
            
            userAccount.RecoveryToken = _assistantService.GenerateRandomString(SharedConstants.ACCOUNT_ACTIVATION_TOKEN_LENGTH);
            userAccount.TokenSetOn = DateTime.UtcNow;

            if (await _accountService.UpdateUserAccount(userAccount) &&
                await SendAccountActivationEmail(userAccount.Username, userAccount.Email, userAccount.RecoveryToken)
            )
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });

            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Error = SharedEnums.HttpStatusCodes.InternalServerError });
        }

        private async Task<bool> SendAccountActivationEmail(string username, string email, string activationToken) {
            using var fileReader = System.IO.File.OpenText($"{ SharedConstants.EMAIL_TEMPLATES_DIRECTORY }AccountActivationEmail.html");
            var accountActivationEmailTemplate = await fileReader.ReadToEndAsync();

            var accountActivationEmailContent = accountActivationEmailTemplate.Replace("[EMAIL_TITLE]", "Activate your account");
            accountActivationEmailContent = accountActivationEmailTemplate.Replace("[USER_NAME]", username);
            accountActivationEmailContent = accountActivationEmailTemplate.Replace("[USER_EMAIL]", email);
            accountActivationEmailContent = accountActivationEmailTemplate.Replace("[ACTIVATION_TOKEN]", activationToken);
            accountActivationEmailContent = accountActivationEmailTemplate.Replace("[VALIDITY_DURATION]", SharedConstants.ACCOUNT_ACTIVATION_EMAIL_VALIDITY_DURATION.ToString());

            var accountActivationEmail = new EmailContent {
                Subject = "Activate your account",
                Body = accountActivationEmailContent,
                ReceiverName = username,
                ReceiverAddress = email
            };

            return await _emailSenderService.SendEmailSingle(accountActivationEmail);
        }

        [HttpPost("authenticate")]
        public async Task<JsonResult> Authenticate(AuthenticationVM authenticationData) {
            var isRequestedByHuman = await _googleRecaptchaService.IsHumanRegistration(authenticationData.RecaptchaToken);
            if (!isRequestedByHuman.Result) return new JsonResult(isRequestedByHuman);

            var authenticationDataVerification = authenticationData.VerifyAuthenticationData();
            if (authenticationDataVerification.Count != 0) {
                var errorMessages = authenticationData.GenerateErrorMessages(authenticationDataVerification);
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errorMessages });
            }

            var (isSuccessful, userAccount) = await _authenticationService.AuthenticateUserAccount(authenticationData);
            if (!isSuccessful) return new JsonResult(new JsonResponse {
                Result = SharedEnums.RequestResults.Failed,
                Message = "No account matches the authentication data.",
                Error = SharedEnums.HttpStatusCodes.NotFound
            });

            var authenticationTimestamp = DateTime.UtcNow;
            var tokenSalt = _assistantService.GenerateRandomString();
            var authenticationToken = Helpers.GenerateSha512Hash(
                $"{ userAccount.Id }{ userAccount.Email }{ Helpers.ConvertToUnixTimestamp(authenticationTimestamp) }{ tokenSalt }"
            );

            var (authenticatedUser, authenticationRecord) = CreateAuthenticatedUserAndRecord(userAccount, authenticationData.TrustedAuth, authenticationData.DeviceInformation);

            var authSavingResult = await _authenticationService.InsertAuthenticationRecord(authenticationRecord);
            if (authSavingResult.HasValue && authSavingResult.Value)
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = authenticatedUser });
            
            HttpContext.Session.Clear();
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "We encountered an issue while establishing your session." });
        }

        [HttpPost("authenticate-session")]
        public async Task<JsonResult> AuthenticateSession(SessionAuthVM sessionAuthData) {
            var authRecord = await _authenticationService.GetLatestAuthRecordForUserAccount(sessionAuthData);
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
                $"{ authRecord.AccountId }{ dbAccount.Email }{ authRecord.AuthTimestamp }{ authRecord.AuthTokenSalt }"
            );

            if (reliableAuthToken.Equals(sessionAuthData.AuthToken)) {
                var (authenticatedUser, authenticationRecord) = CreateAuthenticatedUserAndRecord(dbAccount, sessionAuthData.GetTrustedAuth(), sessionAuthData.DeviceInformation);

                var authSavingResult = await _authenticationService.InsertAuthenticationRecord(authenticationRecord);
                if (authSavingResult.HasValue && authSavingResult.Value)
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = authenticatedUser });
                
                HttpContext.Session.Clear();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "We encountered an issue while establishing your session." });
            }

            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed });
        }

        private KeyValuePair<AuthenticatedUser, AuthRecord> CreateAuthenticatedUserAndRecord(
            Account userAccount, bool trustedAuth, string deviceInformation
        ) {
            var authenticationTimestamp = DateTime.UtcNow;
            var tokenSalt = _assistantService.GenerateRandomString();
            var authenticationToken = Helpers.GenerateSha512Hash(
                $"{ userAccount.Id }{ userAccount.Email }{ Helpers.ConvertToUnixTimestamp(authenticationTimestamp) }{ tokenSalt }"
            );
            
            var authenticatedUser = new AuthenticatedUser {
                AccountId = userAccount.Id,
                AuthToken = authenticationToken
            };
            authenticatedUser.SetTrustedAuth(trustedAuth);
            
            HttpContext.Session.SetString(SharedEnums.SessionKeys.IsUserAuthenticated.GetEnumValue(), "True");
            HttpContext.Session.SetString(SharedEnums.SessionKeys.AuthenticatedUser.GetEnumValue(), JsonConvert.SerializeObject(authenticatedUser));
            HttpContext.Session.SetInt32(nameof(AuthenticatedUser.AccountId), authenticatedUser.AccountId);
            authenticatedUser.SessionId = HttpContext.Session.Id;
            
            var authenticationRecord = new AuthRecord {
                AccountId = userAccount.Id,
                SessionId = HttpContext.Session.Id,
                AuthTokenSalt = tokenSalt,
                AuthTimestamp = Helpers.ConvertToUnixTimestamp(authenticationTimestamp),
                DeviceInformation = deviceInformation,
                TrustedAuth = trustedAuth
            };

            return new KeyValuePair<AuthenticatedUser, AuthRecord>(authenticatedUser, authenticationRecord);
        }

        [HttpGet("sign-out")]
        public JsonResult Unauthenticate() {
            HttpContext.Session.Clear();
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }
    }
}