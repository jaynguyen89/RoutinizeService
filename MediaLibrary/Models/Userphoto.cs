using System;
using System.Collections.Generic;

#nullable disable

namespace MediaLibrary.Models
{
    public partial class Userphoto
    {
        public int Id { get; set; }
        public int? PhotoId { get; set; }
        public int? HidrogenianId { get; set; }
        public bool? IsAvatar { get; set; }
        public bool? IsCover { get; set; }

        public virtual Photo Photo { get; set; }
    }
}
