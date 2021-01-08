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
        public string AuthToken { get; set; }
        public int AuthTimestamp { get; set; }
        public string DeviceInformation { get; set; }

        public virtual Account Account { get; set; }
    }
}
