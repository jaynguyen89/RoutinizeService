using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HelperLibrary;
using HelperLibrary.Shared;

namespace RoutinizeCore.ViewModels.Organization {

    public sealed class WebsiteVM {
        
        public string Title { get; set; }
        
        public string Url { get; set; }

        public List<string> VerifyWebsite() {
            var errors = VerifyTitle();
            errors.AddRange(VerifyUrl());

            return errors;
        }

        private List<string> VerifyTitle() {
            Title = Title?.Trim()?.Replace(SharedConstants.ALL_SPACES, SharedConstants.MONO_SPACE);
            if (!Helpers.IsProperString(Title)) return new List<string>() { "Website title is missing." };
            
            Title = Helpers.CapitalizeFirstLetterOfSentence(Title);
            
            var lenTest = new Regex(@".{3,30}");
            return lenTest.IsMatch(Title) ? default
                                          : new List<string>() { "Website title is too " + (Title.Length > 30 ? "long. Max 20 characters." : "short. Min 3 characters.") };
        }

        private List<string> VerifyUrl() {
            Url = Url?.Trim()?.Replace(SharedConstants.ALL_SPACES, string.Empty);
            if (!Helpers.IsProperString(Url)) return new List<string>() { "Website URL is missing." };

            Url = Url.ToLower();
            var lenTest = new Regex(@".{15,200}");
            if (!lenTest.IsMatch(Url)) return new List<string>() { "Website URL is too " + (Url.Length > 200 ? "long. Max 100 characters." : "short. Min 15 characters.") };
            
            return Uri.IsWellFormedUriString(Url, UriKind.RelativeOrAbsolute) ? default
                                                                              : new List<string>() { "Website URL seems to be invalid." };
        }
    }
}