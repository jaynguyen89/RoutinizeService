using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using AssistantLibrary.Interfaces;
using AssistantLibrary.Models;
using HelperLibrary;
using HelperLibrary.Shared;
using Microsoft.AspNetCore.Mvc;
using MongoLibrary.Interfaces;
using RoutinizeCore.Attributes;
using RoutinizeCore.Services.Interfaces;
using RoutinizeCore.ViewModels;
using RoutinizeCore.ViewModels.Account;
using RoutinizeCore.ViewModels.Challenge;

namespace RoutinizeCore.Controllers {

    [ApiController]
    [Route("account")]
    public sealed class AccountController : ControllerBase {

        private readonly IRoutinizeAccountLogService _accountLogService;
        private readonly IAccountService _accountService;
        private readonly IChallengeService _challengeService;
        private readonly ITwoFactorAuthService _tfaService;
        private readonly IGoogleRecaptchaService _recaptchaService;
        
        private readonly IAssistantService _assistantService;
        private readonly IEmailSenderService _emailSenderService;

        public AccountController(
            IRoutinizeAccountLogService accountLogService,
            IAccountService accountService,
            IChallengeService challengeService,
            IAssistantService assistantService,
            IEmailSenderService emailSenderService,
            ITwoFactorAuthService tfaService,
            IGoogleRecaptchaService recaptchaService
        ) {
            _accountLogService = accountLogService;
            _accountService = accountService;
            _challengeService = challengeService;
            _assistantService = assistantService;
            _emailSenderService = emailSenderService;
            _tfaService = tfaService;
            _recaptchaService = recaptchaService;
        }

        [HttpGet("get-unique-id")]
        public async Task<JsonResult> GetAccountUniqueId([FromHeader] int accountId) {
            var account = await _accountService.GetUserAccountById(accountId);
            return account == null
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = account.UniqueId });
        }

        [HttpPut("change-account-email")]
        [RoutinizeActionFilter]
        public async Task<JsonResult> ChangeAccountEmail(EmailUpdateVM emailUpdateData) {
            var newEmailValidation = emailUpdateData.VerifyNewEmail();
            if (newEmailValidation.Count != 0) {
                var errorMessages = emailUpdateData.GenerateErrorMessages(newEmailValidation);
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errorMessages, Error = SharedEnums.HttpStatusCodes.Conflict });
            }

            var userAccount = await _accountService.GetUserAccountById(emailUpdateData.AccountId);
            if (userAccount == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "Failed to find your account." });

            if (!_assistantService.IsHashMatchesPlainText(userAccount.PasswordHash, emailUpdateData.Password))
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Denied, Message = "Password is incorrect.", Error = SharedEnums.HttpStatusCodes.Forbidden });

            userAccount.Email = emailUpdateData.NewEmail;
            userAccount.EmailConfirmed = false;

            var confirmationToken = Helpers.GenerateRandomString(SharedConstants.ACCOUNT_ACTIVATION_TOKEN_LENGTH);
            userAccount.RecoveryToken = confirmationToken;
            userAccount.TokenSetOn = DateTime.UtcNow;

            await _accountService.StartTransaction();
            var updateResult = await _accountService.UpdateUserAccount(userAccount);
            if (!updateResult) {
                await _accountService.RevertTransaction();

                return new JsonResult(new JsonResponse {
                    Result = SharedEnums.RequestResults.Failed,
                    Message = "An error occurred while updating your account.",
                    Error = SharedEnums.HttpStatusCodes.InternalServerError
                });
            }
            
            var emailUpdateLog = new EmailUpdateLog {
                Activity = nameof(ChangeAccountEmail),
                AccountId = userAccount.Id,
                EmailBeingReplaced = userAccount.Email
            };

            if (!await _accountLogService.InsertRoutinizeAccountLog(emailUpdateLog)) {
                await _accountService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An error occurred while updating your email." });
            }

            using var fileReader = System.IO.File.OpenText($"{ SharedConstants.EMAIL_TEMPLATES_DIRECTORY }EmailUpdateNotificationEmail.html");
            var emailUpdateNotificationContent = await fileReader.ReadToEndAsync();

            emailUpdateNotificationContent = emailUpdateNotificationContent.Replace("[USER_NAME]", userAccount.Username);
            emailUpdateNotificationContent = emailUpdateNotificationContent.Replace("[ACTIVATION_TOKEN]", confirmationToken);
            emailUpdateNotificationContent = emailUpdateNotificationContent.Replace("[VALIDITY_DURATION]", SharedConstants.ACCOUNT_ACTIVATION_EMAIL_VALIDITY_DURATION.ToString());
            emailUpdateNotificationContent = emailUpdateNotificationContent.Replace("[USER_EMAIL]", userAccount.Email);
            
            var emailUpdateEmail = new EmailContent {
                Subject = "Activate your account",
                Body = emailUpdateNotificationContent,
                ReceiverName = userAccount.Username,
                ReceiverAddress = userAccount.Email
            };

            fileReader.Close();
            if (!await _emailSenderService.SendEmailSingle(emailUpdateEmail)) {
                await _accountLogService.RemoveAccountLogEntry(emailUpdateLog);
                await _accountService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Partial, Message = "Failed to send the activation email." });
            }

            await _accountService.CommitTransaction();
            return new JsonResult(new JsonResponse {Result = SharedEnums.RequestResults.Success });
        }

        [HttpPost("verify-new-email")]
        public async Task<JsonResult> VerifyNewEmail() {
            throw new NotImplementedException();
        }

        [HttpGet("get-challenge-question-for-proof/{accountId}")]
        public async Task<JsonResult> GetChallengeQuestionForProof(int accountId) {
            var challengeQuestion = await _challengeService.GetRandomChallengeQuestionForAccount(accountId);
            return challengeQuestion == null
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = challengeQuestion });
        }

        [HttpGet("get-all-challenge-responses/{accountId}")]
        [RoutinizeActionFilter]
        public async Task<JsonResult> GetAllChallengeResponsesByAccount(int accountId) {
            var challengeResponsesByAccount = await _challengeService.GetChallengeResponsesForAccount(accountId);
            return challengeResponsesByAccount == null
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = challengeResponsesByAccount });
        }

        [HttpPut("update-challenge-response")]
        [RoutinizeActionFilter]
        public async Task<JsonResult> UpdateChallengeResponseByAccount(AccountChallengeVM newAccountChallengeData) {
            var currentChallengeResponses = await _challengeService.GetChallengeResponsesForAccount(newAccountChallengeData.AccountId);
            if (currentChallengeResponses == null)
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            var currentRespondedQuestionIds = currentChallengeResponses.Select(challengeResponse => challengeResponse.QuestionId).ToArray();
            var newChallengeResponses =
                newAccountChallengeData.ChallengeResponses
                                       .Where(
                                           challengeResponse => !currentRespondedQuestionIds.Contains(challengeResponse.QuestionId)
                                       )
                                       .ToArray();

            var newRespondedQuestionIds = newAccountChallengeData.ChallengeResponses.Select(challengeResponse => challengeResponse.QuestionId).ToArray();
            var removedChallengeResponses =
                currentChallengeResponses
                    .Where(
                        challengeResponse => !newRespondedQuestionIds.Contains(challengeResponse.QuestionId)
                    )
                    .ToArray();

            var newQuestionResponses = newAccountChallengeData.ChallengeResponses.ToDictionary(
                response => response.QuestionId,
                response => response.Response
            );
            var updatedResponses =
                currentChallengeResponses
                    .Where(
                        challengeResponse =>
                            newRespondedQuestionIds.Contains(challengeResponse.QuestionId) &&
                            !challengeResponse.Response.Equals(newQuestionResponses[challengeResponse.QuestionId])
                    ).ToArray();

            bool? saveResult = null;
            bool? removeResult = null;
            bool? updateResult = null;

            await _accountService.StartTransaction();
            if (newChallengeResponses.Length != 0)
                saveResult = await _challengeService.SaveChallengeRecordsForAccount(newAccountChallengeData.AccountId, newChallengeResponses);
            
            if (saveResult.HasValue && saveResult.Value && removedChallengeResponses.Length != 0)
                removeResult = await _challengeService.DeleteChallengeRecords(removedChallengeResponses);
            
            if (saveResult.HasValue && saveResult.Value && removeResult.HasValue && removeResult.Value && updatedResponses.Length != 0)
                updateResult = await _challengeService.UpdateChallengeRecords(updatedResponses);

            if (updateResult.HasValue && updateResult.Value) {
                await _accountService.CommitTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
            }

            await _accountService.RevertTransaction();
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed });
        }

        [HttpGet("enable-two-factor/{accountId}")]
        [RoutinizeActionFilter]
        public async Task<JsonResult> EnableTwoFactorAuthentication(int accountId) {
            var twoFactorSecretKey = Helpers.GenerateRandomString(SharedConstants.TWO_FA_SECRET_KEY_LENGTH);
            var userAccount = await _accountService.GetUserAccountById(accountId);

            userAccount.TwoFactorEnabled = true;
            userAccount.TwoFaSecretKey = twoFactorSecretKey;
            
            var updateResult = await _accountService.UpdateUserAccount(userAccount);
            if (!updateResult) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue occurred while updating data." });
            
            var authenticator = _tfaService.GetTwoFactorAuthSetup(twoFactorSecretKey, userAccount.Email);
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = authenticator });
        }

        [HttpPut("disable-two-factor")]
        [RoutinizeActionFilter]
        public async Task<JsonResult> DisableTwoFactorAuthentication(TwoFaUpdateVM tfaUpdateData) {
            var isRequestedByHuman = await _recaptchaService.IsHumanRegistration(tfaUpdateData.RecaptchaToken);
            if (!isRequestedByHuman.Result) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Error = SharedEnums.HttpStatusCodes.ImATeapot });
            
            var userAccount = await _accountService.GetUserAccountById(tfaUpdateData.AccountId);
            if (userAccount == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });

            if (!_assistantService.IsHashMatchesPlainText(userAccount.PasswordHash, tfaUpdateData.Password))
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "Password is incorrect.", Error = SharedEnums.HttpStatusCodes.Forbidden });

            userAccount.TwoFactorEnabled = false;
            userAccount.TwoFaSecretKey = null;

            var updateResult = await _accountService.UpdateUserAccount(userAccount);
            if (!updateResult) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
            
            using var fileReader = System.IO.File.OpenText($"{ SharedConstants.EMAIL_TEMPLATES_DIRECTORY }TwoFaDisabledNotificationEmail.html");
            var twoFaDisabledNotificationEmailContent = await fileReader.ReadToEndAsync();
            twoFaDisabledNotificationEmailContent = twoFaDisabledNotificationEmailContent.Replace("[USER_NAME]", userAccount.Username);

            var accountActivationEmail = new EmailContent {
                Subject = "Activate your account",
                Body = twoFaDisabledNotificationEmailContent,
                ReceiverName = userAccount.Username,
                ReceiverAddress = userAccount.Email
            };

            fileReader.Close();
            await _emailSenderService.SendEmailSingle(accountActivationEmail);
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }

        [HttpGet("get-all-challenge-questions")]
        [RoutinizeActionFilter]
        public async Task<JsonResult> GetAllChallengeQuestions() {
            var challengeQuestions = await _challengeService.GetChallengeQuestions();
            return challengeQuestions == null
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = challengeQuestions });
        }

        [HttpPost("add-fcm-token")]
        [RoutinizeActionFilter]
        public async Task<JsonResult> AddFcmToken([NotNull] int accountId, [FromHeader] string fcmToken) {
            var result = await _accountService.SaveFcmToken(accountId, fcmToken);
            return !result.HasValue || !result.Value
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }
    }
}