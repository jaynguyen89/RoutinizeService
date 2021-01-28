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
                var (_, requestAccountId) = requestHeaders.Single(header => header.Key.Equals(nameof(AuthenticatedUser.AccountId)));
                var (_, requestAuthToken) = requestHeaders.Single(header => header.Key.Equals(nameof(AuthenticatedUser.AuthToken)));

                var serviceProvider = context.HttpContext.RequestServices;
                var authenticationService = serviceProvider.GetService<IAuthenticationService>();

                var authRecord = authenticationService.GetLatestAuthRecordForUserAccount(int.Parse(requestAccountId.ToString())).Result;
                var reliableAuthToken = Helpers.GenerateSha512Hash(
                    $"{ authRecord.AccountId }{ authRecord.AuthTimestamp }{ authRecord.AuthTokenSalt }"
                );
                
                if (!requestAuthToken.Equals(reliableAuthToken))
                    context.Result = new RedirectToActionResult(
                        nameof(AuthenticationController.UnauthenticationResult),
                        nameof(AuthenticationController),
                        new { actionFilterResults = SharedEnums.ActionFilterResults.UnauthenticatedRequest }
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