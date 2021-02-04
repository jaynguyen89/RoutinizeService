using System.Collections.Generic;
using HelperLibrary;
using HelperLibrary.Shared;

namespace RoutinizeCore.ViewModels.Todo {

    public class SharedTodoVM {
        
        public Models.Todo Todo { get; set; }
        
        public int SharedToId { get; set; } // CollaboratorId in Collaboration table
        
        public string Message { get; set; }
        
        public SharedEnums.Permissions Permission { get; set; }

        public List<string> VerifySharedTodoData() {
            var todoErrors = Todo.VerifyTodoData();

            if (!Helpers.IsProperString(Message)) Message = null;
            else if (Message?.Length > 150) todoErrors.Add("Message is too long. Max 150 characters.");

            return todoErrors;
        }
    }
}