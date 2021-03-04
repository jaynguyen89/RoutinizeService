using System;
using HelperLibrary;

namespace RoutinizeCore.Models {

    public partial class RandomIdea {

        public string[] VerifyContent() {
            if (!Helpers.IsProperString(Content))
                return new[] { "Idea has no content." };

            return Content.Length > 4000 ? new[] { "Content is too long. Max 4000 characters." }
                                         : Array.Empty<string>();
        }
    }
}