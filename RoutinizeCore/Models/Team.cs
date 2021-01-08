using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class Team
    {
        public Team()
        {
            ProjectIterations = new HashSet<ProjectIteration>();
            TeamMembers = new HashSet<TeamMember>();
            TeamRequests = new HashSet<TeamRequest>();
            TeamTasks = new HashSet<TeamTask>();
        }

        public int Id { get; set; }
        public int? CoverImageId { get; set; }
        public int? ProjectId { get; set; }
        public string UniqueCode { get; set; }
        public string TeamName { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? CreatedById { get; set; }

        public virtual Attachment CoverImage { get; set; }
        public virtual User CreatedBy { get; set; }
        public virtual Project Project { get; set; }
        public virtual ICollection<ProjectIteration> ProjectIterations { get; set; }
        public virtual ICollection<TeamMember> TeamMembers { get; set; }
        public virtual ICollection<TeamRequest> TeamRequests { get; set; }
        public virtual ICollection<TeamTask> TeamTasks { get; set; }
    }
}
