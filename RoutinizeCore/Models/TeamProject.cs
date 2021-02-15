using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class TeamProject
    {
        public int Id { get; set; }
        public int TeamId { get; set; }
        public int ProjectId { get; set; }
        public string Description { get; set; }
        public DateTime AssignedOn { get; set; }

        public virtual Project Project { get; set; }
        public virtual Team Team { get; set; }
    }
}
