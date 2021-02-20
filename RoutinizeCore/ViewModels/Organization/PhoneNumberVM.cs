using System.Collections.Generic;
using System.Text.RegularExpressions;
using HelperLibrary;
using HelperLibrary.Shared;

namespace RoutinizeCore.ViewModels.Organization {

    public sealed class PhoneNumberVM {
        
        public string Title { get; set; }
        
        public string PhoneNumber { get; set; }
        
        public string Extension { get; set; }

        public List<string> VerifyPhoneNumberData() {
            var errors = VerifyTitle();
            errors.AddRange(VerifyPhoneNumber());
            errors.AddRange(VerifyExtension());

            return errors;
        }
        
        private List<string> VerifyTitle() {
            Title = Title?.Trim()?.Replace(SharedConstants.AllSpaces, SharedConstants.MonoSpace);
            if (!Helpers.IsProperString(Title)) return new List<string>() { "Phone number title is missing." };
            
            Title = Helpers.CapitalizeFirstLetterOfSentence(Title);
            
            var lenTest = new Regex(@".{3,30}");
            return lenTest.IsMatch(Title) ? default
                                          : new List<string>() { "Phone number title is too " + (Title.Length > 30 ? "long. Max 20 characters." : "short. Min 3 characters.") };
        }

        private List<string> VerifyPhoneNumber() {
            PhoneNumber = PhoneNumber?.Trim()?.Replace(SharedConstants.AllSpaces, SharedConstants.MonoSpace);
            if (!Helpers.IsProperString(PhoneNumber)) return new List<string>() { "Phone number is missing" };

            PhoneNumber = PhoneNumber.ToUpper();
            var formatTest = new Regex(@"^\+?(\d[\d-. ]+)?(\([\d-. ]+\))?[\d-. ]+\d$");
            if (!formatTest.IsMatch(PhoneNumber)) return new List<string>() { "Phone number seems to be invalid." };
            
            var lenTest = new Regex(@".{10,20}");
            return lenTest.IsMatch(PhoneNumber) ? default
                                                : new List<string>() { "Phone number is too " + (PhoneNumber.Length > 20 ? "long. Max 20 characters." : "short. Min 10 characters.") };
        }

        private List<string> VerifyExtension() {
            if (!Helpers.IsProperString(Extension)) {
                Extension = null;
                return default;
            }
            
            Extension = Extension.Trim().Replace(SharedConstants.AllSpaces, string.Empty);
            var regex = new Regex(@"^\d{1,}$");
            return regex.IsMatch(Extension) ? default : new List<string>() { "Phone extension should be a number." };
        }
    }
}