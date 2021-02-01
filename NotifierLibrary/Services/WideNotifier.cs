using System.IO;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using NotifierLibrary.Interfaces;

namespace NotifierLibrary.Services {

    public class WideNotifier : IWideNotifier {
        
        private static readonly string ROOT_DIR = Path.GetDirectoryName(Directory.GetCurrentDirectory()) + @"/Assets/";

        public WideNotifier() {
            FirebaseApp.Create(new AppOptions() {
                Credential = GoogleCredential.FromFile($"{ ROOT_DIR }routinize-firebase-admin-sdk-service-account.json")
            });
        }
    }
}