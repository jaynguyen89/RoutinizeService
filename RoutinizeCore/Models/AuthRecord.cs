using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class AuthRecord
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string SessionId { get; set; }
        public bool TrustedAuth { get; set; }
        public string AuthTokenSalt { get; set; }
        public long AuthTimestamp { get; set; }
        public string DeviceInformation { get; set; }

        public virtual Account Account { get; set; }
    }
}
