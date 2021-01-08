using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class TaskPermission
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public byte Role { get; set; }
        public byte Permission { get; set; }
    }
}
