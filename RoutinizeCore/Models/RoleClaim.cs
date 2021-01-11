using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class RoleClaim
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public int ClaimerId { get; set; }
        public bool AllowAddMember { get; set; }
        public bool AllowApproveMember { get; set; }
        public bool AllowRemoveMember { get; set; }
        public bool AllowEditTeamInfo { get; set; }

        public virtual TeamMember Claimer { get; set; }
        public virtual Role Role { get; set; }
    }
}
