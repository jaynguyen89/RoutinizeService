using System.Collections.Generic;
using HelperLibrary;
using RoutinizeCore.ViewModels.Challenge;

namespace RoutinizeCore.ViewModels.AccountRecovery {

    public sealed class ForgotPasswordVM {
        
        // Only used in SendNewPasswordResetEmail,
        // When this property is used, Email and Username will be null
        public int AccountId { get; set; }
        
        public string Email { get; set; }
        
        public string Username { get; set; }
        
        public string RecaptchaToken { get; set; }
        
        public string VerifyForgotPasswordData() {
            Username = Username?.Trim().ToLower();
            Email = Email?.Trim().ToLower();

            if (!Helpers.IsProperString(Email) &&
                !Helpers.IsProperString(Username)
            )
                return "Email or username must be provided.";

            return null;
        }
    }
}