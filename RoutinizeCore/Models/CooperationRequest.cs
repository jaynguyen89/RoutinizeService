using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class CooperationRequest
    {
        public int Id { get; set; }
        public int RequestedById { get; set; }
        public string RequestedByType { get; set; }
        public int RequestedToId { get; set; }
        public string RequestedToType { get; set; }
        public string Message { get; set; }
        public DateTime RequestedOn { get; set; }
        public bool? IsAccepted { get; set; }
        public DateTime? AcceptedOn { get; set; }
        public int? AcceptedById { get; set; }
        public DateTime? RejectedOn { get; set; }
        public int? RejectedById { get; set; }
        public string ResponderSignatures { get; set; }
        public string AcceptanceNote { get; set; }
    }
}
