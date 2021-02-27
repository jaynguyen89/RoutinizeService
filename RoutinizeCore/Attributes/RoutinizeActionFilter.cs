using System;
using System.Linq;
using HelperLibrary;
using HelperLibrary.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using RoutinizeCore.Controllers;
using RoutinizeCore.Services.Interfaces;
using RoutinizeCore.ViewModels.Authentication;

namespace RoutinizeCore.Attributes {

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class RoutinizeActionFilter : ActionFilterAttribute {

        public RoutinizeActionFilter() { }

        public override void OnActionExecuting(ActionExecutingContext context) {
            base.OnActionExecuting(context);

            try {
                var requestHeaders = context.HttpContext.Request.Headers;
                var (_, accountIdInRequest) = requestHeaders.Single(header => header.Key.Equals(nameof(AuthenticatedUser.AccountId)));
                var (_, authTokenInRequest) = requestHeaders.Single(header => header.Key.Equals(nameof(AuthenticatedUser.AuthToken)));

                var serviceProvider = context.HttpContext.RequestServices;
                var authenticationService = serviceProvider.GetService<IAuthenticationService>();

                var authRecord = authenticationService.GetLatestAuthRecordForUserAccount(int.Parse(accountIdInRequest.ToString())).Result;
                var reliableAuthToken = Helpers.GenerateSha512Hash(
                    $"{ authRecord.AccountId }{ authRecord.AuthTimestamp }{ authRecord.AuthTokenSalt }"
                );
                
                if (!authTokenInRequest.Equals(reliableAuthToken))
                    context.Result = new RedirectToActionResult(
                        nameof(AuthenticationController.UnauthenticationResult),
                        nameof(AuthenticationController),
                        new { actionFilterResults = SharedEnums.ActionFilterResults.UnauthenticatedRequest }
                    );

                var (_, userIdInRequest) = requestHeaders.Single(header => header.Key.Equals(nameof(AuthenticatedUser.UserId)));

                var userService = serviceProvider.GetService<IUserService>();
                var userAccount = userService.GetAccountByUserId(int.Parse(userIdInRequest.ToString())).Result;
                
                if (userAccount.Id != int.Parse(accountIdInRequest.ToString()))
                    context.Result = new RedirectToActionResult(
                        nameof(AuthenticationController.UnauthenticationResult),
                        nameof(AuthenticationController),
                        new { actionFilterResults = SharedEnums.ActionFilterResults.IrrelevantAuthError }
                    );
            }
            catch (Exception) {
                context.Result = new RedirectToActionResult(
                    nameof(AuthenticationController.UnauthenticationResult),
                    nameof(AuthenticationController),
                    new { actionFilterResults = SharedEnums.ActionFilterResults.RequestProcessingError }
                );
            }
        }
    }
}