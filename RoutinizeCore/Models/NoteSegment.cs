using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class NoteSegment
    {
        public int Id { get; set; }
        public int NoteId { get; set; }
        public string Body { get; set; }
        public bool IsShared { get; set; }
        public DateTime? DeletedOn { get; set; }

        public virtual Note Note { get; set; }
    }
}
