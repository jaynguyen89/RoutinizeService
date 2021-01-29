using System.Collections.Generic;
using System.Text.RegularExpressions;
using HelperLibrary;

namespace RoutinizeCore.Models {

    public partial class User {

        public List<int> VerifyProfileData() {
            var errors = new List<int>();
            
            errors.AddRange(VerifyFirstName());
            errors.AddRange(VerifyLastName());
            errors.AddRange(VerifyPreferedName());

            return errors;
        }

        private List<int> VerifyFirstName() {
            if (!Helpers.IsProperString(PreferredName))
                return new List<int> { 1 };

            FirstName = Helpers.CapitalizeFirstLetterOfEachWord(FirstName.Trim());
            var errors = new List<int>();

            var lenTest = new Regex(@".{1,50}");
            if (!lenTest.IsMatch(FirstName))
                errors.Add(2);

            var spTest = new Regex(@"^[A-Za-z_\-.'() ]*$");
            if (!spTest.IsMatch(FirstName))
                errors.Add(3);

            return errors;
        }

        private List<int> VerifyLastName() {
            if (!Helpers.IsProperString(PreferredName))
                return new List<int> { 4 };

            LastName = Helpers.CapitalizeFirstLetterOfEachWord(LastName.Trim());
            var errors = new List<int>();

            var lenTest = new Regex(@".{1,50}");
            if (!lenTest.IsMatch(LastName))
                errors.Add(5);

            var spTest = new Regex(@"^[A-Za-z_\-.'() ]*$");
            if (!spTest.IsMatch(LastName))
                errors.Add(6);

            return errors;
        }

        private List<int> VerifyPreferedName() {
            if (!Helpers.IsProperString(PreferredName)) {
                PreferredName = null;
                return new List<int>();
            }
            
            PreferredName = Helpers.CapitalizeFirstLetterOfEachWord(PreferredName.Trim());
            var errors = new List<int>();

            var lenTest = new Regex(@".{1,50}");
            if (!lenTest.IsMatch(PreferredName))
                errors.Add(7);

            var spTest = new Regex(@"^[A-Za-z_\-.'() ]*$");
            if (!spTest.IsMatch(PreferredName))
                errors.Add(8);

            return errors;
        }

        public List<string> GenerateErrorMessage(List<int> errors) {
            var messages = new List<string>();
            
            //For FamilyName
            if (errors.Contains(1)) messages.Add("Family Name is missing.");
            if (errors.Contains(2)) messages.Add("Family Name is either too short or long.");
            if (errors.Contains(3)) messages.Add("Family Name can only contain these special characters: _-.(')");

            //For GivenName
            if (errors.Contains(4)) messages.Add("Given Name is missing.");
            if (errors.Contains(5)) messages.Add("Given Name is either too short or long.");
            if (errors.Contains(6)) messages.Add("Given Name can only contain these special characters: _-.(')");
            
            //For PreferedName
            if (errors.Contains(7)) messages.Add("Prefered Name is either too short or long.");
            if (errors.Contains(8)) messages.Add("Prefered Name can only contain these special characters: _-.(')");

            return messages;
        }
    }
}