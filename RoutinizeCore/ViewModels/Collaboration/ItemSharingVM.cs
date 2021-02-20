using System;
using HelperLibrary;
using HelperLibrary.Shared;

namespace RoutinizeCore.ViewModels.Collaboration {

    public class ItemSharingVM {
        
        public int ItemId { get; set; }
        
        public string ItemType { get; set; }
        
        public int SharedToUserId { get; set; }
        
        public string Message { get; set; }

        public string[] ValidateMessage() {
            if (!Helpers.IsProperString(Message)) {
                Message = null;
                return Array.Empty<string>();
            }

            Message = Message?.Trim();
            Message = Message?.Replace(SharedConstants.AllSpaces, SharedConstants.MonoSpace);
            if (Message?.Length > 150) return new[] { "Message is too long. Max 150 characters." };
            
            Message = Helpers.CapitalizeFirstLetterOfSentence(Message);
            return Array.Empty<string>();
        }
    }
}