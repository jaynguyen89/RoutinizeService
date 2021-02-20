using System;
using System.Collections.Generic;
using HelperLibrary;
using HelperLibrary.Shared;

namespace RoutinizeCore.Models {

    public partial class PositionTitle {

        public string[] VerifyPositionTitleData() {
            var errors = VerifyTitle();
            errors.AddRange(VerifyDescription());

            return errors.ToArray();
        }

        private List<string> VerifyTitle() {
            if (!Helpers.IsProperString(Title)) return new List<string> { "Position title is missing." };

            Title = Title.Trim().Replace(SharedConstants.AllSpaces, SharedConstants.MonoSpace);
            return Title.Length > 100 ? new List<string> { "Position title is too long. Max 100 characters." } : default;
        }

        private List<string> VerifyDescription() {
            if (!Helpers.IsProperString(Description)) {
                Description = null;
                return default;
            }

            Description = Description.Trim().Replace(SharedConstants.AllSpaces, SharedConstants.MonoSpace);
            Description = Helpers.CapitalizeFirstLetterOfSentence(Description);
            return Description.Length > 300 ? new List<string> { "Position description is too long. Max 300 characters." } : default;
        }

        public static PositionTitle GetDefaultManagerialTitle() {
            return new PositionTitle {
                Title = SharedConstants.DefaultPositionTitle,
                Description = "Chief Executive Officer: Head of the organization, assigned to the user who added the organization.",
                AddedOn = DateTime.UtcNow
            };
        }
    }
}