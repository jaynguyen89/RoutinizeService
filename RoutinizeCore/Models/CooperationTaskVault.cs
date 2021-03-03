using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class CooperationTaskVault
    {
        public CooperationTaskVault()
        {
            TaskVaultItems = new HashSet<TaskVaultItem>();
        }

        public int Id { get; set; }
        public int CooperationId { get; set; }
        public int PossessedByCooperatorId { get; set; }
        public int AssignedToCooperatorId { get; set; }
        public int AssociatedWithDepartmentId { get; set; }
        public string TaskVaultName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOn { get; set; }

        public virtual CooperationParticipant AssignedToCooperator { get; set; }
        public virtual Department AssociatedWithDepartment { get; set; }
        public virtual Cooperation Cooperation { get; set; }
        public virtual CooperationParticipant PossessedByCooperator { get; set; }
        public virtual ICollection<TaskVaultItem> TaskVaultItems { get; set; }
    }
}
