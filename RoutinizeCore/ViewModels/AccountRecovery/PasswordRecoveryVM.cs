using System.Collections.Generic;
using System.Text.RegularExpressions;
using HelperLibrary;
using RoutinizeCore.ViewModels.Challenge;

namespace RoutinizeCore.ViewModels.AccountRecovery {

    public class PasswordRecoveryVM {
        
        public int AccountId { get; set; }
        
        public string NewPassword { get; set; }
        
        public string NewPasswordConfirm { get; set; }
        
        public ChallengeResponseVM ChallengeResponse { get; set; }
        
        public List<int> VerifyNewPassword(string password = null) {
            if (!Helpers.IsProperString(NewPassword) ||
                !Helpers.IsProperString(NewPasswordConfirm)
            )
                return new List<int> { 8 };

            var errors = new List<int>();

            if (NewPassword != NewPasswordConfirm)
                errors.Add(9);

            var lenTest = new Regex(@".{6,20}");
            if (!lenTest.IsMatch(NewPassword))
                errors.Add(10);

            var lowTest = new Regex(@"[a-z]+");
            if (!lowTest.IsMatch(NewPassword))
                errors.Add(11);

            var capTest = new Regex(@"[A-Z]+");
            if (!capTest.IsMatch(NewPassword))
                errors.Add(12);

            var digitTest = new Regex(@"[\d]+");
            if (!digitTest.IsMatch(NewPassword))
                errors.Add(13);

            if (!errors.Contains(10) && NewPassword.Contains(" "))
                errors.Add(14);

            var spTest = new Regex(@"[!@#$%^&*_+\.]+");
            if (!spTest.IsMatch(NewPassword))
                errors.Add(15);

            return errors;
        }
        
        public List<string> GenerateErrorMessages(List<int> errors) {
            var messages = new List<string>();
            
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