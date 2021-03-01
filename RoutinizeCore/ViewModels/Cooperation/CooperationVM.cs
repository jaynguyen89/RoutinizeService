using System;
using Newtonsoft.Json;
using RoutinizeCore.ViewModels.User;

namespace RoutinizeCore.ViewModels.Cooperation {

    public class CooperationVM {
        
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public DateTime? StartedOn { get; set; }

        public static implicit operator CooperationVM(Models.Cooperation cooperation) {
            return new() {
                Id = cooperation.Id,
                Name = cooperation.Name,
                StartedOn = cooperation.StartedOn
            };
        }
    }

    public sealed class CooperationDetailVM : CooperationVM {
        
        public string TermsAndConditions { get; set; }
        
        public bool IsInEffect { get; set; }
        
        public DateTime? EndedOn { get; set; }
        
        public bool AllowAnyoneToResponseRequest { get; set; }
        
        public UserVM[] ConfidedRequestResponders { get; set; }
        
        public RequestAcceptancePolicyVM RequestAcceptancePolicy { get; set; }

        public static implicit operator CooperationDetailVM(Models.Cooperation cooperation) {
            return new() {
                Id = cooperation.Id,
                Name = cooperation.Name,
                TermsAndConditions = cooperation.TermsAndConditions,
                StartedOn = cooperation.StartedOn,
                IsInEffect = cooperation.IsInEffect,
                EndedOn = cooperation.EndedOn,
                AllowAnyoneToResponseRequest = cooperation.AllowAnyoneToRespondRequest,
                RequestAcceptancePolicy = JsonConvert.DeserializeObject<RequestAcceptancePolicyVM>(cooperation.RequestAcceptancePolicy)
            };
        }
    }

    //Todo: add audit for activate/deactivate to cooperation and participant details
    public sealed class ActivationAudit {
        
    }
}