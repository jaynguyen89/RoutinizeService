using System;
using System.Collections.Generic;
using HelperLibrary;
using HelperLibrary.Shared;
using RoutinizeCore.ViewModels.User;

namespace RoutinizeCore.ViewModels.Cooperation {

    public class DbRequestAcceptanceVM {
        
        public int ResponderId { get; set; }
        
        public bool IsAccepted { get; set; }
    }
    
    public class DbSignatureRecordVM : DbRequestAcceptanceVM {
        
        public string Signature { get; set; }
        
        public string Note { get; set; }
        
        public long Timestamp { get; set; }

        public static implicit operator DbSignatureRecordVM(RequestResponseVM response) {
            return new() {
                ResponderId = response.ResponderId,
                IsAccepted = response.IsAccepted,
                Note = response.Note
            };
        }
    }

    public sealed class SignaturePoolVM {

        public List<DbSignatureRecordVM> UserSignatures { get; set; } = new();

        public List<DbOrganizationSignatureRecordVM> OrganizationSignatures { get; set; } = new();
    }

    public sealed class DbOrganizationSignatureRecordVM : DbRequestAcceptanceVM {
        
        public int OrganizationId { get; set; }
        
        public DbSignatureRecordVM Signature { get; set; }
    }

    public sealed class SignatureVM : DbSignatureRecordVM {

        public UserVM Responder { get; set; }
    }

    public sealed class RequestResponseVM : DbRequestAcceptanceVM {
        
        public int RequestId { get; set; }
        
        public string Note { get; set; }

        public string[] VerifyResponse() {
            if (!Helpers.IsProperString(Note)) {
                Note = SharedConstants.Na;
                return Array.Empty<string>();
            }

            return Note.Length > 300 ? new[] { "Response note is too long. Max 300 characters." }
                                     : Array.Empty<string>();
        }
    }

    public sealed class RequestAcceptancePolicyVM {
        
        //AcceptIfAllParticipantsAccept and RejectIfOneParticipantReject can be true together
        //if AcceptBasingOnMajority is true, AcceptIfAllParticipantsAccept and RejectIfOneParticipantReject are ignored
        
        public bool AcceptIfAllParticipantsAccept { get; set; }
        
        public bool RejectIfOneParticipantReject { get; set; }
        
        public bool AcceptBasingOnMajority { get; set; }
        
        public bool EarlyAutoAccept { get; set; } //If accept% is greater than reject% and no-response% altogether, then just accept without waiting for the rest of responders
        
        public double PercentageOfMajority { get; set; } //From 50% to 90%
        
        public double RangeForTurningToBeDetermined { get; set; } //From 0% to 10%
    }
}