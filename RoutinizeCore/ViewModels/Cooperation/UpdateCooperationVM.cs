using System;
using System.Collections.Generic;
using HelperLibrary;

namespace RoutinizeCore.ViewModels.Cooperation {

    public class UpdateCooperationVM {
        
        public int CooperationId { get; set; }
        
        public string Name { get; set; }
        
        public bool RequireSigning { get; set; }
    }

    public sealed class TermsAndConditionsVM : UpdateCooperationVM {
        
        public string TermsAndConditions { get; set; }

        public string[] VerifyData() {
            if (!Helpers.IsProperString(TermsAndConditions))
                TermsAndConditions = null;
            
            if (Helpers.IsProperString(Name))
                return Name.Length > 100
                    ? new[] {"Cooperation name is too long. Max 100 characters."}
                    : Array.Empty<string>();
            
            Name = null;
            return Array.Empty<string>();
        }
    }

    public sealed class CooperationPreferenceVM : UpdateCooperationVM {
        
        public bool AllowAnyoneToResponseRequest { get; set; }
        
        public int[] ConfidedRequestResponderIds { get; set; }
        
        public RequestAcceptancePolicyVM RequestAcceptancePolicy { get; set; }

        public string[] VerifyPreference() {
            if (!AllowAnyoneToResponseRequest && ConfidedRequestResponderIds.Length == 0)
                return new[] { "Please specify the confided responders to cooperation requests or allow anyone to response." };

            return RequestAcceptancePolicy.VerifyAcceptancePolicy().ToArray();
        }
    }
    
    public sealed class RequestAcceptancePolicyVM {
        
        //AcceptIfAllParticipantsAccept and RejectIfOneParticipantReject can be true together
        //if AcceptBasingOnMajority is true, AcceptIfAllParticipantsAccept and RejectIfOneParticipantReject are ignored
        
        public bool AcceptIfAllParticipantsAccept { get; set; }
        
        public bool RejectIfOneParticipantReject { get; set; }
        
        public bool AcceptIfOneParticipantAccept { get; set; }
        
        public bool AcceptBasingOnMajority { get; set; }
        
        public bool EarlyAutoAccept { get; set; } //If accept% is greater than reject% and no-response% altogether, then just accept without waiting for the rest of responders
        
        public double PercentageOfMajority { get; set; } //From 50% to 99%
        
        public double RangeForTurningToBeDetermined { get; set; } //From 0% to 10%

        public List<string> VerifyAcceptancePolicy() {
            if (!AcceptBasingOnMajority) return new List<string>();

            var errors = new List<string>();
            if (PercentageOfMajority < 0.5 || PercentageOfMajority > 0.99) errors.Add("Please set Majority Percentage within 50% and 99%.");
            if (RangeForTurningToBeDetermined < 0 || RangeForTurningToBeDetermined > 0.1) errors.Add("Please set Range For Determining within 0 and 10%.");

            return errors;
        }
    }
}