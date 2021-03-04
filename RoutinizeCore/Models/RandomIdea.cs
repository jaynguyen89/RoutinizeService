using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class RandomIdea
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? GroupId { get; set; }
        public string Content { get; set; }
        public DateTime AddedOn { get; set; }
        public DateTime? DeletedOn { get; set; }

        public virtual ContentGroup Group { get; set; }
        public virtual User User { get; set; }
    }
}
