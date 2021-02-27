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
}