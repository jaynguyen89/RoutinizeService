using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HelperLibrary;
using HelperLibrary.Shared;
using Newtonsoft.Json;

namespace RoutinizeCore.ViewModels.Organization {

    public sealed class DepartmentContactVM {

        public List<PhoneNumberVM> PhoneNumbers { get; set; }
        
        public List<string> Websites { get; set; }

        public List<string> VerifyContactDetails() {
            var phoneNumberError = PhoneNumbers
                                   .Select(number => number.VerifyPhoneNumberData())
                                   .FirstOrDefault(errors => errors.Count != 0)
                                   ??
                                   new List<string>();

            var websiteError = Websites.Select(
                                           website => {
                                               var url = website?.Trim()?.Replace(SharedConstants.AllSpaces, string.Empty);
                                               if (!Helpers.IsProperString(url)) return new List<string>() { "Website URL is missing." };

                                               url = url.ToLower();
                                               var lenTest = new Regex(@".{15,200}");
                                               if (!lenTest.IsMatch(url))
                                                   return new List<string>() { "Website URL is too " + (url.Length > 200 ? "long. Max 100 characters." : "short. Min 15 characters.") };

                                               return Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute)
                                                   ? new List<string>()
                                                   : new List<string>() { "Website URL seems to be invalid." };
                                           }
                                       )
                                       .FirstOrDefault(errors => errors.Count != 0);

            return phoneNumberError.Concat(websiteError ?? new List<string>()).ToList();
        }

        public static implicit operator DepartmentContactVM(string contactDetails) {
            return JsonConvert.DeserializeObject<DepartmentContactVM>(contactDetails);
        }
    }
}