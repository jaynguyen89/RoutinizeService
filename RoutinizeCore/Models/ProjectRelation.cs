using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class ProjectRelation
    {
        public int Id { get; set; }
        public int RelationshipId { get; set; }
        public int FromProjectId { get; set; }
        public int ToProjectId { get; set; }
        public DateTime CreatedOn { get; set; }

        public virtual Project FromProject { get; set; }
        public virtual Relationship Relationship { get; set; }
        public virtual Project ToProject { get; set; }
    }
}
