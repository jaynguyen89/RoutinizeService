using System;
using HelperLibrary;
using RoutinizeCore.ViewModels.Organization;
using RoutinizeCore.ViewModels.User;

namespace RoutinizeCore.ViewModels.Cooperation {

    public class CooperationRequestVM {
        
        public int RequestedById { get; set; }
        
        public string RequestedByType { get; set; }
        
        public int RequestedToId { get; set; }
        
        public string RequestedToType { get; set; }
        
        public string Message { get; set; }

        public string[] VerifyRequestData() {
            if (!Helpers.IsProperString(Message)) {
                Message = null;
                return Array.Empty<string>();
            }

            return Message.Length > 2000
                ? new[] { "Message is too long. Max 2000 characters." }
                : Array.Empty<string>();
        }
    }

    public sealed class CooperationRequestDetailVM {
        
        public int Id { get; set; }
        
        public CommunicatorVM Communicator { get; set; } //Sender or Receiver of the Request

        public bool AllowRespond { get; set; }
        
        public sealed class CommunicatorVM {
            //User, Organization, Cooperation can't be null or having value at once
            
            public UserVM User { get; set; }
            
            public OrganizationVM Organization { get; set; }
            
            public CooperationVM Cooperation { get; set; }
            
            public string Type { get; set; } //nameof(User) || nameof(Organization) || nameof(Cooperation)
        }
    }
}