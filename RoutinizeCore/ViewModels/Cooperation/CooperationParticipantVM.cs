using System;
using RoutinizeCore.Models;
using RoutinizeCore.ViewModels.Organization;
using RoutinizeCore.ViewModels.User;

namespace RoutinizeCore.ViewModels.Cooperation {

    public sealed class CooperationParticipantVM {
        
        public int Id { get; set; }
        
        public UserVM UserParticipant { get; set; }
        
        public OrganizationVM OrganizationParticipant { get; set; }
        
        public string Type { get; set; }
        
        public bool IsActive { get; set; }
        
        public DateTime? LeftOn { get; set; }

        public static implicit operator CooperationParticipantVM(CooperationParticipant participant) {
            return new() {
                Id = participant.Id,
                Type = participant.ParticipantType,
                IsActive = participant.IsActive,
                LeftOn = participant.LeftOn
            };
        }
    }
}