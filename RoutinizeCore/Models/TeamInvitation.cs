using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class TeamInvitation
    {
        public int Id { get; set; }
        public int InvitationSentById { get; set; }
        public int InvitationSentToId { get; set; }
        public DateTime IssuedOn { get; set; }
        public DateTime? ValidUntil { get; set; }
        public string Message { get; set; }
        public bool IsAccepted { get; set; }
        public DateTime? AcceptedOn { get; set; }

        public virtual User InvitationSentBy { get; set; }
        public virtual User InvitationSentTo { get; set; }
    }
}
