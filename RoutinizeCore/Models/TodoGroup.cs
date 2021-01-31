using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class TodoGroup
    {
        public TodoGroup()
        {
            Todos = new HashSet<Todo>();
        }

        public int Id { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public int CreatedById { get; set; }
        public DateTime CreatedOn { get; set; }

        public virtual User CreatedBy { get; set; }
        public virtual ICollection<Todo> Todos { get; set; }
    }
}
