using System.Collections.Generic;
using System.Linq;
using HelperLibrary;
using HelperLibrary.Shared;

namespace RoutinizeCore.ViewModels.Note {

    public sealed class SharedNoteVM {
        
        public Models.Note Note { get; set; }
        
        public int SharedToId { get; set; }
        
        public string Message { get; set; }
        
        public SharedEnums.Permissions Permission { get; set; }
        
        public List<string> VerifySharedNoteData() {
            var noteErrors = Note.ValidateNoteAndSegments().ToList();

            if (!Helpers.IsProperString(Message)) Message = null;
            else if (Message?.Length > 300) noteErrors.Add("Message is too long. Max 300 characters.");

            return noteErrors;
        }
    }
}