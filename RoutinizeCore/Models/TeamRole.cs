using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class TeamRole
    {
        public TeamRole()
        {
            TeamRoleClaims = new HashSet<TeamRoleClaim>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
        public byte EnumValue { get; set; }
        public string ForTeamIds { get; set; }
        public DateTime AddedOn { get; set; }
        public bool AllowAddProjectIteration { get; set; }
        public bool AllowEditProjectIteration { get; set; }
        public bool AllowDeleteProjectIteration { get; set; }
        public bool AllowAddTask { get; set; }
        public bool AllowEditTask { get; set; }
        public bool AllowDeleteTask { get; set; }
        public bool AllowAssignTask { get; set; }
        public bool AllowConstraintTask { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<TeamRoleClaim> TeamRoleClaims { get; set; }
    }
}
