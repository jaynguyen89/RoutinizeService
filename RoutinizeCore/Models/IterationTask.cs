using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class IterationTask
    {
        public int Id { get; set; }
        public int IterationId { get; set; }
        public int TaskId { get; set; }
        public string TaskType { get; set; }
        public DateTime AssignedOn { get; set; }
        public string Message { get; set; }

        public virtual ProjectIteration Iteration { get; set; }
    }
}
