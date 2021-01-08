using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class TeamMember
    {
        public int Id { get; set; }
        public int TeamId { get; set; }
        public int MemberId { get; set; }
        public byte MemberRole { get; set; }
        public byte Permission { get; set; }

        public virtual User Member { get; set; }
        public virtual Team Team { get; set; }
    }
}
