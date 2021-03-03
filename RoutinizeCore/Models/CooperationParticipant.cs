using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class CooperationParticipant
    {
        public CooperationParticipant()
        {
            CooperationTaskVaultAssignedToCooperators = new HashSet<CooperationTaskVault>();
            CooperationTaskVaultPossessedByCooperators = new HashSet<CooperationTaskVault>();
            ParticipantReturnRequests = new HashSet<ParticipantReturnRequest>();
            SigningCheckers = new HashSet<SigningChecker>();
        }

        public int Id { get; set; }
        public int CooperationId { get; set; }
        public int ParticipantId { get; set; }
        public string ParticipantType { get; set; }
        public DateTime ParticipatedOn { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LeftOn { get; set; }

        public virtual Cooperation Cooperation { get; set; }
        public virtual ICollection<CooperationTaskVault> CooperationTaskVaultAssignedToCooperators { get; set; }
        public virtual ICollection<CooperationTaskVault> CooperationTaskVaultPossessedByCooperators { get; set; }
        public virtual ICollection<ParticipantReturnRequest> ParticipantReturnRequests { get; set; }
        public virtual ICollection<SigningChecker> SigningCheckers { get; set; }
    }
}
