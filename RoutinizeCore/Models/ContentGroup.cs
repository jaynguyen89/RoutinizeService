using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class ContentGroup
    {
        public ContentGroup()
        {
            GroupShares = new HashSet<GroupShare>();
            Notes = new HashSet<Note>();
            Todos = new HashSet<Todo>();
        }

        public int Id { get; set; }
        public string GroupName { get; set; }
        public string GroupOfType { get; set; }
        public string Description { get; set; }
        public bool IsShared { get; set; }
        public int CreatedById { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? DeletedOn { get; set; }

        public virtual User CreatedBy { get; set; }
        public virtual ICollection<GroupShare> GroupShares { get; set; }
        public virtual ICollection<Note> Notes { get; set; }
        public virtual ICollection<Todo> Todos { get; set; }
    }
}
