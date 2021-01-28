﻿using System;
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

    public sealed class AccountController : ControllerBase {

        private readonly IRoutinizeAccountLogService _accountLogService;
        private readonly IAccountService _accountService;
        private readonly IUserService _userService;
        private readonly IChallengeService _challengeService;
        private readonly ITwoFactorAuthService _tfaService;
        private readonly IGoogleRecaptchaService _recaptchaService;
        
        private readonly IAssistantService _assistantService;
        private readonly IEmailSenderService _emailSenderService;

        public AccountController(
            IRoutinizeAccountLogService accountLogService,
            IAccountService accountService,
            IUserService userService,
            IChallengeService challengeService,
            IAssistantService assistantService,
            IEmailSenderService emailSenderService,
            ITwoFactorAuthService tfaService,
            IGoogleRecaptchaService recaptchaService
        ) {
            _accountLogService = accountLogService;
            _accountService = accountService;
            _userService = userService;
            _challengeService = challengeService;
            _assistantService = assistantService;
            _emailSenderService = emailSenderService;
            _tfaService = tfaService;
            _recaptchaService = recaptchaService;
        }

        [HttpPost("change-account-email")]
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

            var emailUpdateLog = new EmailUpdateLog {
                Activity = nameof(ChangeAccountEmail),
                AccountId = userAccount.Id,
                EmailBeingReplaced = userAccount.Email
            };

            if (!await _accountLogService.InsertRoutinizeAccountLog(emailUpdateLog))
                return new JsonResult(new JsonResponse {Result = SharedEnums.RequestResults.Failed, Message = "An error occurred while updating your email."});

            userAccount.Email = emailUpdateData.NewEmail;
            userAccount.EmailConfirmed = false;

            var confirmationToken = Helpers.GenerateRandomString(SharedConstants.ACCOUNT_ACTIVATION_TOKEN_LENGTH);
            userAccount.RecoveryToken = confirmationToken;
            userAccount.TokenSetOn = DateTime.UtcNow;
            
            var updateResult = await _accountService.UpdateUserAccount(userAccount);
            if (!updateResult) {
                await _accountLogService.RemoveAccountLogEntry(emailUpdateLog);
                
                return new JsonResult(new JsonResponse {
                    Result = SharedEnums.RequestResults.Failed,
                    Message = "An error occurred while updating your account.",
                    Error = SharedEnums.HttpStatusCodes.InternalServerError
                });
            }

            using var fileReader = System.IO.File.OpenText($"{ SharedConstants.EMAIL_TEMPLATES_DIRECTORY }EmailUpdateNotificationEmail.html");
            var emailUpdateNotificationTemplate = await fileReader.ReadToEndAsync();

            var emailUpdateNotificationContent = emailUpdateNotificationTemplate.Replace("[USER_NAME]", userAccount.Username);
            emailUpdateNotificationContent = emailUpdateNotificationTemplate.Replace("[ACTIVATION_TOKEN]", confirmationToken);
            emailUpdateNotificationContent = emailUpdateNotificationTemplate.Replace("[VALIDITY_DURATION]", SharedConstants.ACCOUNT_ACTIVATION_EMAIL_VALIDITY_DURATION.ToString());
            emailUpdateNotificationContent = emailUpdateNotificationTemplate.Replace("[USER_EMAIL]", userAccount.Email);
            
            var emailUpdateEmail = new EmailContent {
                Subject = "Activate your account",
                Body = emailUpdateNotificationContent,
                ReceiverName = userAccount.Username,
                ReceiverAddress = userAccount.Email
            };

            fileReader.Close();
            if (!await _emailSenderService.SendEmailSingle(emailUpdateEmail))
                return new JsonResult(new JsonResponse {Result = SharedEnums.RequestResults.Partial, Message = "Failed to send the activation email."});
            
            return new JsonResult(new JsonResponse {Result = SharedEnums.RequestResults.Success });
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

        [HttpPost("update-challenge-response")]
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
            
            if (newChallengeResponses.Length != 0)
                saveResult = await _challengeService.SaveChallengeRecordsForAccount(newAccountChallengeData.AccountId, newChallengeResponses);
            
            if (saveResult.HasValue && saveResult.Value && removedChallengeResponses.Length != 0)
                removeResult = await _challengeService.DeleteChallengeRecords(removedChallengeResponses);
            
            if (saveResult.HasValue && saveResult.Value && removeResult.HasValue && removeResult.Value && updatedResponses.Length != 0)
                updateResult = await _challengeService.UpdateChallengeRecords(updatedResponses);

            if (updateResult.HasValue && updateResult.Value) return new JsonResult(new JsonResponse {Result = SharedEnums.RequestResults.Success});
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

        [HttpPost("disable-two-factor")]
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
            var twoFaDisabledNotificationEmailTemplate = await fileReader.ReadToEndAsync();
            var twoFaDisabledNotificationEmailContent = twoFaDisabledNotificationEmailTemplate.Replace("[USER_NAME]", userAccount.Username);

            var accountActivationEmail = new EmailContent {
                Subject = "Activate your account",
                Body = twoFaDisabledNotificationEmailContent,
                ReceiverName = userAccount.Username,
                ReceiverAddress = userAccount.Email
            };

            fileReader.Close();
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
    }
}