using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class ParticipantReturnRequest
    {
        public int Id { get; set; }
        public int CooperationParticipantId { get; set; }
        public string Message { get; set; }
        public DateTime RequestedOn { get; set; }
        public bool? IsAccepted { get; set; }
        public int RespondedById { get; set; }
        public DateTime? RespondedOn { get; set; }
        public string RespondNote { get; set; }

        public virtual CooperationParticipant CooperationParticipant { get; set; }
        public virtual User RespondedBy { get; set; }
    }
}
