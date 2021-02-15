using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class Team
    {
        public Team()
        {
            TeamDepartments = new HashSet<TeamDepartment>();
            TeamMembers = new HashSet<TeamMember>();
            TeamProjects = new HashSet<TeamProject>();
            TeamRequests = new HashSet<TeamRequest>();
            TeamTasks = new HashSet<TeamTask>();
        }

        public int Id { get; set; }
        public int? CoverImageId { get; set; }
        public string UniqueCode { get; set; }
        public string TeamName { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? CreatedById { get; set; }

        public virtual Attachment CoverImage { get; set; }
        public virtual User CreatedBy { get; set; }
        public virtual ICollection<TeamDepartment> TeamDepartments { get; set; }
        public virtual ICollection<TeamMember> TeamMembers { get; set; }
        public virtual ICollection<TeamProject> TeamProjects { get; set; }
        public virtual ICollection<TeamRequest> TeamRequests { get; set; }
        public virtual ICollection<TeamTask> TeamTasks { get; set; }
    }
}
