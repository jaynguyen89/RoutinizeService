using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class SigningChecker
    {
        public int Id { get; set; }
        public int CooperationParticipantId { get; set; }
        public string ForActivity { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsValid { get; set; }
        public DateTime? InvalidOn { get; set; }

        public virtual CooperationParticipant CooperationParticipant { get; set; }
    }
}
