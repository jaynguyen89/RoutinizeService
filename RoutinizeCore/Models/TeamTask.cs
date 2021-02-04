using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class TeamTask
    {
        public int Id { get; set; }
        public int TeamId { get; set; }
        public int TaskId { get; set; }
        public string TaskType { get; set; }
        public DateTime AssignedOn { get; set; }
        public string Message { get; set; }

        public virtual Team Team { get; set; }
    }
}
