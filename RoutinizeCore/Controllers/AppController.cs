using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using HelperLibrary;
using Microsoft.AspNetCore.Mvc;
using NotifierLibrary.Assets;
using NotifierLibrary.Interfaces;
using RoutinizeCore.Models;
using RoutinizeCore.Services.Interfaces;

namespace RoutinizeCore.Controllers {

    public class AppController : ControllerBase {

        private readonly IUserNotifier _userNotifier;

        protected const string TokenNotifierName = "[NotifierName]";

        protected AppController(IUserNotifier userNotifier) {
            _userNotifier = userNotifier;
        }

        protected async Task<bool> SendNotification(
            User fromUser, int toUserId, UserNotification notification, IUserService userService,[AllowNull] string fcmToken = null
        ) {
            if (!Helpers.IsProperString(fcmToken)) {
                var collaboratorAccount = await userService.GetAccountByUserId(toUserId);
                if (collaboratorAccount == null) return false;

                fcmToken = collaboratorAccount.FcmToken;
            }

            var fromUserFullname = Helpers.IsProperString(fromUser.PreferredName)
                ? fromUser.PreferredName
                : $"{ fromUser.FirstName } { fromUser.LastName }";

            if (notification.Message.Contains(TokenNotifierName))
                notification.Message = notification.Message.Replace("", fromUserFullname);

            return await _userNotifier.NotifyUserSingle(fcmToken, notification);
        }
    }
}