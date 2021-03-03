using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class TaskVaultItem
    {
        public int Id { get; set; }
        public int TaskVaultId { get; set; }
        public int ItemId { get; set; }
        public string ItemType { get; set; }
        public DateTime AddedOn { get; set; }
        public int AddedByUserId { get; set; }

        public virtual User AddedByUser { get; set; }
        public virtual CooperationTaskVault TaskVault { get; set; }
    }
}
