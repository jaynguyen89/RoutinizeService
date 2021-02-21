using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class Cooperation
    {
        public Cooperation()
        {
            CooperationParticipants = new HashSet<CooperationParticipant>();
            CooperationTaskVaults = new HashSet<CooperationTaskVault>();
            DepartmentAccesses = new HashSet<DepartmentAccess>();
        }

        public int Id { get; set; }
        public string TermsAndConditions { get; set; }
        public DateTime? StartedOn { get; set; }
        public DateTime? EndedOn { get; set; }
        public bool IsLocked { get; set; }
        public bool AllowAnyoneToUnlock { get; set; }
        public string ParticipantIdsToAllowUnlock { get; set; }
        public string ConfidedRequestResponderIds { get; set; }
        public bool AllowAnyoneToRespondRequest { get; set; }
        public string ParticipantIdsToAllowRespondRequest { get; set; }
        public string AgreementSigners { get; set; }

        public virtual ICollection<CooperationParticipant> CooperationParticipants { get; set; }
        public virtual ICollection<CooperationTaskVault> CooperationTaskVaults { get; set; }
        public virtual ICollection<DepartmentAccess> DepartmentAccesses { get; set; }
    }
}
