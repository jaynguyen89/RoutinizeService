using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class TaskRelation
    {
        public int Id { get; set; }
        public int RelationshipId { get; set; }
        public int TaskId { get; set; }
        public string TaskType { get; set; }
        public int RelatedToId { get; set; }
        public string RelatedToType { get; set; }
        public DateTime CreatedOn { get; set; }

        public virtual Relationship Relationship { get; set; }
    }
}
