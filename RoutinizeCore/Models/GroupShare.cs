using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class GroupShare
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string SharedToType { get; set; }
        public int SharedToId { get; set; }
        public int SharedById { get; set; }
        public DateTime SharedOn { get; set; }
        public string SideNotes { get; set; }

        public virtual ContentGroup Group { get; set; }
        public virtual User SharedBy { get; set; }
    }
}
