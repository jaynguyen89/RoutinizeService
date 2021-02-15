using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class Project
    {
        public Project()
        {
            ProjectIterations = new HashSet<ProjectIteration>();
            ProjectRelationFromProjects = new HashSet<ProjectRelation>();
            ProjectRelationToProjects = new HashSet<ProjectRelation>();
            TeamProjects = new HashSet<TeamProject>();
        }

        public int Id { get; set; }
        public int CoverImageId { get; set; }
        public string ProjectName { get; set; }
        public string ProjectCode { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? CreatedById { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ActuallyFinishedOn { get; set; }

        public virtual Attachment CoverImage { get; set; }
        public virtual User CreatedBy { get; set; }
        public virtual ICollection<ProjectIteration> ProjectIterations { get; set; }
        public virtual ICollection<ProjectRelation> ProjectRelationFromProjects { get; set; }
        public virtual ICollection<ProjectRelation> ProjectRelationToProjects { get; set; }
        public virtual ICollection<TeamProject> TeamProjects { get; set; }
    }
}
