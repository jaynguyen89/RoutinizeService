using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class Relationship
    {
        public Relationship()
        {
            ProjectRelations = new HashSet<ProjectRelation>();
            TaskRelations = new HashSet<TaskRelation>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public byte EnumValue { get; set; }
        public string OppositeName { get; set; }

        public virtual ICollection<ProjectRelation> ProjectRelations { get; set; }
        public virtual ICollection<TaskRelation> TaskRelations { get; set; }
    }
}
