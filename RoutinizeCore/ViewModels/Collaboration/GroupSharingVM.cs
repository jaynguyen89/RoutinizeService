using System;
using HelperLibrary;
using HelperLibrary.Shared;
using RoutinizeCore.Models;

namespace RoutinizeCore.ViewModels.Collaboration {

    public class GroupSharingVM {
        
        public int GroupId { get; set; }
        
        public string GroupOfType { get; set; } //Leave null from request, value set by controller's action
        
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

            if (GroupOfType.Equals(nameof(RandomIdea)))
                return new[] { "Group of your ideas is not shareable." };
            
            Message = Helpers.CapitalizeFirstLetterOfSentence(Message);
            return Array.Empty<string>();
        }
    }
}