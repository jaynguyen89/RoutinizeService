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

        public virtual ICollection<CooperationParticipant> CooperationParticipants { get; set; }
        public virtual ICollection<CooperationTaskVault> CooperationTaskVaults { get; set; }
        public virtual ICollection<DepartmentAccess> DepartmentAccesses { get; set; }
    }
}
