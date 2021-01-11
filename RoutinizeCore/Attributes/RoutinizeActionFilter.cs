using System;
using HelperLibrary;
using HelperLibrary.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RoutinizeCore.Controllers;

namespace RoutinizeCore.Attributes {

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Class)]
    public sealed class RoutinizeActionFilter : ActionFilterAttribute {

        public RoutinizeActionFilter() { }

        public override void OnActionExecuting(ActionExecutingContext context) {
            base.OnActionExecuting(context);

            var isRequestAuthenticated = bool.Parse(context.HttpContext.Session.GetString(SharedEnums.SessionKeys.IsUserAuthenticated.GetEnumValue()));
            if (!isRequestAuthenticated)
                context.Result = new RedirectToActionResult(
                    nameof(AuthenticationController.UnauthenticationResult),
                    nameof(AuthenticationController),
                    new { actionFilterResults = SharedEnums.ActionFilterResults.UnauthenticatedRequest }
                );
        }
    }
}