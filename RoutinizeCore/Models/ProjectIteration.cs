using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class ProjectIteration
    {
        public ProjectIteration()
        {
            IterationTasks = new HashSet<IterationTask>();
        }

        public int Id { get; set; }
        public int TeamId { get; set; }
        public string IterationName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ActuallyFinishedOn { get; set; }

        public virtual Team Team { get; set; }
        public virtual ICollection<IterationTask> IterationTasks { get; set; }
    }
}
