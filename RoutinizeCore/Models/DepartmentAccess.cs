using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class DepartmentAccess
    {
        public int Id { get; set; }
        public int CooperationId { get; set; }
        public int FromParticipantId { get; set; }
        public int AccessGivenToParticipantId { get; set; }
        public string AccessibleDepartmentIds { get; set; }

        public virtual Cooperation Cooperation { get; set; }
    }
}
