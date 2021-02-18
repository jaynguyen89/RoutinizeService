using System;
using HelperLibrary;
using HelperLibrary.Shared;

namespace RoutinizeCore.ViewModels.Collaboration {

    public sealed class CollabRequestVM {
        
        public string UniqueId { get; set; }
        
        public string Message { get; set; }

        public string[] ValidateMessage() {
            if (!Helpers.IsProperString(Message)) {
                Message = null;
                return Array.Empty<string>();
            }

            Message = Message?.Trim();
            Message = Message?.Replace(SharedConstants.ALL_SPACES, SharedConstants.MONO_SPACE);
            if (Message?.Length > 300) return new[] { "Message is too long. Max 300 characters." };
            
            Message = Helpers.CapitalizeFirstLetterOfSentence(Message);
            return Array.Empty<string>();
        }
    }
}