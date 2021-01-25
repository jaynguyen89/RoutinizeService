using System;
using System.Threading.Tasks;
using AssistantLibrary.Interfaces;
using AssistantLibrary.Models;
using HelperLibrary.Shared;
using Microsoft.AspNetCore.Mvc;
using MongoLibrary.Interfaces;
using RoutinizeCore.Attributes;
using RoutinizeCore.Services.Interfaces;
using RoutinizeCore.ViewModels;
using RoutinizeCore.ViewModels.Account;

namespace RoutinizeCore.Controllers {

    public sealed class AccountController : ControllerBase {

        private readonly IRoutinizeAccountLogService _accountLogService;
        private readonly IAccountService _accountService;
        private readonly IUserService _userService;
        private readonly IChallengeService _challengeService;
        
        private readonly IAssistantService _assistantService;
        private readonly IEmailSenderService _emailSenderService;

        public AccountController(
            IRoutinizeAccountLogService accountLogService,
            IAccountService accountService,
            IUserService userService,
            IChallengeService challengeService,
            IAssistantService assistantService,
            IEmailSenderService emailSenderService
        ) {
            _accountLogService = accountLogService;
            _accountService = accountService;
            _userService = userService;
            _challengeService = challengeService;
            _assistantService = assistantService;
            _emailSenderService = emailSenderService;
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

            var confirmationToken = _assistantService.GenerateRandomString(SharedConstants.ACCOUNT_ACTIVATION_TOKEN_LENGTH);
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
    }
}