using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class UserRsaKey
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public bool SignatureEnabled { get; set; }
        public string KeyModulus { get; set; }
        public string KeyExponent { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public bool IsActive { get; set; }
        public DateTime GeneratedOn { get; set; }

        public virtual User User { get; set; }
    }
}
