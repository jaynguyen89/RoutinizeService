using System;
using System.Threading.Tasks;
using AssistantLibrary.Interfaces;
using HelperLibrary;
using HelperLibrary.Shared;
using Microsoft.AspNetCore.Mvc;
using MongoLibrary.Interfaces;
using RoutinizeCore.Services;
using RoutinizeCore.Services.Interfaces;
using RoutinizeCore.ViewModels;

namespace RoutinizeCore.Controllers {

    [ApiController]
    [Route("authentication")]
    public sealed class AuthenticationController : CacheServiceBase {
        
        private readonly IRoutinizeCoreLogService _coreLogService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IAccountService _accountService;
        private readonly IUserService _userService;

        private readonly IGoogleRecaptchaService _googleRecaptchaService;
        private readonly IEmailSenderService _emailSenderService;

        public AuthenticationController(
            IRoutinizeCoreLogService coreLogService,
            IAuthenticationService authenticationService,
            IAccountService accountService,
            IUserService userService,
            IGoogleRecaptchaService googleRecaptchaService,
            IEmailSenderService emailSenderService
        ) {
            _coreLogService = coreLogService;
            _authenticationService = authenticationService;
            _accountService = accountService;
            _userService = userService;
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
            if (!Helpers.IsStringNotNullOrBlankOrSpace(email))
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "Missing data." });
            
            email = email.Trim().ToLower();
            var emailAvailable = await _authenticationService.IsRegistrationEmailAvailable(email);
            
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = emailAvailable });
        }

        [HttpGet("check-username-availability/{username}")]
        public async Task<JsonResult> CheckUsernameAvailability(string username) {
            if (!Helpers.IsStringNotNullOrBlankOrSpace(username))
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "Missing data." });

            username = username.Trim().ToLower();
            var usernameAvailable = await _authenticationService.IsUsernameAvailable(username);
            
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = usernameAvailable });
        }
    }
}