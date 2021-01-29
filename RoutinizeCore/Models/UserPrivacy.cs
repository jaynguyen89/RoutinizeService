using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class UserPrivacy
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public byte AddressPolicy { get; set; }
        public byte NamePolicy { get; set; }
        public byte PhonePolicy { get; set; }
        public byte UsernamePolicy { get; set; }

        public virtual User User { get; set; }
    }
}
