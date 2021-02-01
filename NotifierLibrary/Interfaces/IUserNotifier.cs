using System.Threading.Tasks;
using NotifierLibrary.Assets;

namespace NotifierLibrary.Interfaces {

    public interface IUserNotifier {

        Task<bool> NotifyUserSingle(string fcmToken, UserNotification message);

        Task<bool> NotifyUsersMultiple(string[] fcmTokens, UserNotification[] messages);
    }
}