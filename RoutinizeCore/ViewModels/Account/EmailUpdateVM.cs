using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HelperLibrary;
using HelperLibrary.Shared;
using Swashbuckle.AspNetCore.Annotations;

namespace RoutinizeCore.ViewModels.Account {

    [SwaggerSchema(Required = new [] { "AccountId", "NewEmail", "NewEmailConfirm", "Password" })]
    public sealed class EmailUpdateVM {
        
        [SwaggerSchema("The new email to replace the current one.")]
        public string NewEmail { get; set; }
        
        [SwaggerSchema("Must match newEmail.")]
        public string NewEmailConfirm { get; set; }
        
        [SwaggerSchema("The account's current password.")]
        public string Password { get; set; }
        
        public List<int> VerifyNewEmail() {
            NewEmail = NewEmail.Trim().ToLower();

            if (!Helpers.IsProperString(NewEmail))
                return new List<int> { 0 };

            var errors = new List<int>();

            var lenTest = new Regex(@".{10,50}");
            if (!lenTest.IsMatch(NewEmail))
                errors.Add(1);

            var fmTest = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            if (!fmTest.IsMatch(NewEmail) || SharedConstants.InvalidEmailTokens.Any(NewEmail.Contains))
                errors.Add(2);

            if (errors.Count == 0) {
                var domain = NewEmail.Split("@")[1];
                if (domain.Split(".").Length > 3)
                    errors.Add(3);
            }
            
            if (errors.Count == 0 && !NewEmail.Equals(NewEmailConfirm))
                errors.Add(4);

            return errors;
        }

        public List<string> GenerateErrorMessages(List<int> errors) {
            var errorMessages = new List<string>();
            
            if (errors.Contains(0)) errorMessages.Add("Missing input for new email.");
            if (errors.Contains(1)) errorMessages.Add("Email is either too short or long. Min 10, Max 50 characters.");
            if (errors.Contains(2) || errors.Contains(3)) errorMessages.Add("The email you enter seems to be invalid.");
            if (errors.Contains(4)) errorMessages.Add("The new email and confirmation do not match.");

            return errorMessages;
        }
    }
}