using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using HelperLibrary;
using HelperLibrary.Shared;

namespace RoutinizeCore.Models {

    public partial class Todo {

        public List<string> VerifyTodoData([NotNull] bool isUpdating = false) {
            var errorMessages = new List<string>();

            Title = Title?.Trim();
            if (!Helpers.IsProperString(Title)) Title = null;
            else if (Title?.Length > 100) errorMessages.Add("Title is too long. Max 100 characters.");
            
            Description = Description?.Trim();
            if (!Helpers.IsProperString(Description)) Description = null;
            else if (Description?.Length > 250) errorMessages.Add("Description is too long. Max 250 characters.");
            
            Details = Details?.Trim();
            if (!Helpers.IsProperString(Details)) Details = null;
            else if (Details?.Length > 4000) errorMessages.Add("Details is too long. Max 4000 characters.");
            
            if (Title == null && Description == null && Details == null)
                errorMessages.Add("Please enter some information for one of Title, Description or Details.");

            CoverImage = CoverImage?.Trim();
            if (CoverImage != null) Regex.Replace(CoverImage, SharedConstants.ALL_SPACES, string.Empty);
            
            if (!isUpdating) CreatedOn = DateTime.UtcNow;

            return errorMessages;
        }
    }
}