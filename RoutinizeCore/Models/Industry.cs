using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class Industry
    {
        public Industry()
        {
            InverseParent = new HashSet<Industry>();
            Organizations = new HashSet<Organization>();
        }

        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual Industry Parent { get; set; }
        public virtual ICollection<Industry> InverseParent { get; set; }
        public virtual ICollection<Organization> Organizations { get; set; }
    }
}
