using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HelperLibrary;
using HelperLibrary.Shared;

namespace RoutinizeCore.ViewModels.Authentication {

    public sealed class RegisterAccountVM {
        
        public string Email { get; set; }
        
        public string Username { get; set; }
        
        public string Password { get; set; }
        
        public string PasswordConfirm { get; set; }
        
        public string RecaptchaToken { get; set; }
        
        public List<int> VerifyEmail() {
            Email = Email.Trim().ToLower();

            if (!Helpers.IsProperString(Email))
                return new List<int> { 0 };

            var errors = new List<int>();

            var lenTest = new Regex(@".{10,100}");
            if (!lenTest.IsMatch(Email))
                errors.Add(1);

            var fmTest = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            if (!fmTest.IsMatch(Email) || SharedConstants.INVALID_EMAIL_TOKENS.Any(Email.Contains))
                errors.Add(2);

            if (errors.Count == 0) {
                var domain = Email.Split("@")[1];
                if (domain.Split(".").Length > 3)
                    errors.Add(3);
            }

            return errors;
        }

        //Only "trim" whitespaces in the UserName, keep the resulted string as is
        public List<int> VerifyUserName() {
            if (!Helpers.IsProperString(Username))
                return new List<int> { 4 };

            Username = Username.Trim();
            var errors = new List<int>();

            var lenTest = new Regex(@".{3,20}");
            if (!lenTest.IsMatch(Username))
                errors.Add(5);

            var spTest = new Regex(@"^[0-9A-Za-z_\-.]*$");
            if (!spTest.IsMatch(Username))
                errors.Add(6);

            if (errors.Count == 0 && (SharedConstants.INVALID_EMAIL_TOKENS.Any(Username.Contains) ||
                "_-.".Contains(Username[0]) || "_-.".Contains(Username[^1])))
                errors.Add(7);

            return errors;
        }

        public List<int> VerifyPassword(string password = null) {
            if (!Helpers.IsProperString(Password) ||
                !Helpers.IsProperString(PasswordConfirm)
            )
                return new List<int> { 8 };

            var errors = new List<int>();

            if (Password != PasswordConfirm)
                errors.Add(9);

            var lenTest = new Regex(@".{6,20}");
            if (!lenTest.IsMatch(Password))
                errors.Add(10);

            var lowTest = new Regex(@"[a-z]+");
            if (!lowTest.IsMatch(Password))
                errors.Add(11);

            var capTest = new Regex(@"[A-Z]+");
            if (!capTest.IsMatch(Password))
                errors.Add(12);

            var digitTest = new Regex(@"[\d]+");
            if (!digitTest.IsMatch(Password))
                errors.Add(13);

            if (!errors.Contains(10) && Password.Contains(" "))
                errors.Add(14);

            var spTest = new Regex(@"[!@#$%^&*_+\.]+");
            if (!spTest.IsMatch(Password))
                errors.Add(15);

            return errors;
        }
        
        public List<string> GenerateErrorMessages(List<int> errors) {
            var messages = new List<string>();

            //For Email
            if (errors.Contains(0)) messages.Add("Email is missing.");
            if (errors.Contains(1)) messages.Add($"Email is too { (Email.Length < 10 ? "short. Min 10 characters" : "long. Max 100 characters.") }.");
            if (errors.Contains(2) || errors.Contains(3)) messages.Add("The email seems to be invalid.");

            //For UserName
            if (errors.Contains(4)) messages.Add("Username is missing.");
            if (errors.Contains(5)) messages.Add($"Username is too { (Username.Length < 3 ? "short. Min 3 characters" : "long. Max 20 characters.") }.");
            if (errors.Contains(6) || errors.Contains(7)) messages.Add("The username contains invalid character.");

            //For Password
            if (errors.Contains(8)) messages.Add("Password and/or Confirm Password fields are missing inputs.");
            if (errors.Contains(9)) messages.Add("Password and Confirm Password do not match.");
            if (errors.Contains(10)) messages.Add("Password should be 6 to 20 characters in length.");
            if (errors.Contains(11)) messages.Add("Password must contain at least 1 lowercase character.");
            if (errors.Contains(12)) messages.Add("Password must contain at least 1 uppercase character.");
            if (errors.Contains(13)) messages.Add("Password must contain at least 1 digit.");
            if (errors.Contains(14)) messages.Add("Password must NOT contain white-space.");
            if (errors.Contains(15)) messages.Add("Password must contain at least 1 of these special characters: !@#$%^&*_+.");

            return messages;
        }
    }
}