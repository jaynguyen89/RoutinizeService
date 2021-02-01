using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using NotifierLibrary.Assets;
using NotifierLibrary.Interfaces;

namespace NotifierLibrary.Services {

    public class UserNotifier : IUserNotifier {
        
        private static readonly string ROOT_DIR = Path.GetDirectoryName(Directory.GetCurrentDirectory()) + @"/Assets/";

        public UserNotifier() {
            FirebaseApp.Create(new AppOptions() {
                Credential = GoogleCredential.FromFile($"{ ROOT_DIR }routinize-firebase-admin-sdk-service-account.json")
            });
        }

        public async Task<bool> NotifyUserSingle(string fcmToken, UserNotification message) {
            var notification = new Message {
                Token = fcmToken,
                Data = new Dictionary<string, string>() {
                    { message.Title, message.Message }
                }
            };

            var response = await FirebaseMessaging.DefaultInstance.SendAsync(notification);
            throw new NotImplementedException();
        }

        public async Task<bool> NotifyUsersMultiple(string[] fcmTokens, UserNotification[] messages) {
            throw new System.NotImplementedException();
        }
    }
}