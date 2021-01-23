using System.Collections.Generic;
using HelperLibrary;

namespace RoutinizeCore.ViewModels.Authentication {

    public sealed class AuthenticationVM {
        
        public string Username { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public bool TrustedAuth { get; set; }

        public string RecaptchaToken { get; set; }
        
        public string DeviceInformation { get; set; }

        public List<int> VerifyAuthenticationData() {
            var errors = new List<int>();

            Username = Username?.Trim().ToLower();
            Email = Email?.Trim().ToLower();

            if (!Helpers.IsProperString(Email) &&
                !Helpers.IsProperString(Username)
            )
                errors.Add(0);

            if (!Helpers.IsProperString(Password))
                errors.Add(1);

            return errors; 
        }

        public List<string> GenerateErrorMessages(List<int> validation) {
            var messages = new List<string>();

            if (validation.Contains(0)) messages.Add("Please enter your username or email to login.");
            if (validation.Contains(1)) messages.Add("Please enter your password to login.");

            return messages;
        }
    }
}