using System;
using HelperLibrary;

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

    public sealed class CooperationRequestDetailVM : CooperationRequestVM {
        
        public bool AllowRespond { get; set; }
    }
}