using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class Role
    {
        public Role()
        {
            RoleClaims = new HashSet<RoleClaim>();
        }

        public int Id { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
        public byte EnumValueClient { get; set; }

        public virtual ICollection<RoleClaim> RoleClaims { get; set; }
    }
}
