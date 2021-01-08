using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class TeamRequest
    {
        public int Id { get; set; }
        public int TeamId { get; set; }
        public int RequestedById { get; set; }
        public DateTime RequestedOn { get; set; }
        public DateTime? ValidUntil { get; set; }
        public string Message { get; set; }
        public bool IsAccepted { get; set; }
        public DateTime? AcceptedOn { get; set; }

        public virtual User RequestedBy { get; set; }
        public virtual Team Team { get; set; }
    }
}
