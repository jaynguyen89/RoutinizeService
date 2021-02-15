using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class CooperationParticipant
    {
        public int Id { get; set; }
        public int CooperationId { get; set; }
        public int ParticipantId { get; set; }
        public string ParticipantType { get; set; }
        public DateTime ParticipatedOn { get; set; }
        public bool IsActive { get; set; }

        public virtual Cooperation Cooperation { get; set; }
    }
}
