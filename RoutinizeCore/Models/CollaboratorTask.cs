using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class CollaboratorTask
    {
        public int Id { get; set; }
        public int CollaborationId { get; set; }
        public int TaskId { get; set; }
        public string TaskType { get; set; }
        public byte Permission { get; set; }
        public DateTime AssignedOn { get; set; }
        public string Message { get; set; }

        public virtual Collaboration Collaboration { get; set; }
    }
}
