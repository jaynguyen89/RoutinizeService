using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HelperLibrary;

namespace RoutinizeCore.Models {

    public partial class TodoGroup {

        public List<string> VerifyTodoGroupData([NotNull] bool isUpdating = false) {
            var errorMessages = new List<string>();

            GroupName = GroupName?.Trim();
            if (!Helpers.IsProperString(GroupName)) errorMessages.Add("Please enter the name of Todo Group.");
            else if (GroupName?.Length > 50) errorMessages.Add("Group Name is too long. Max 50 characters.");

            Description = Description?.Trim();
            if (!Helpers.IsProperString(Description)) Description = null;
            else if (Description?.Length > 150) errorMessages.Add("Description is too long. Max 150 characters.");
            
            if (errorMessages.Count == 0 && !isUpdating) CreatedOn = DateTime.UtcNow;

            return errorMessages;
        }
    }
}