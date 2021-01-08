using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class Note
    {
        public Note()
        {
            NoteSegments = new HashSet<NoteSegment>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public bool IsShared { get; set; }
        public bool Emphasized { get; set; }
        public string Title { get; set; }
        public DateTime CreatedOn { get; set; }
        public string DeletedOn { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<NoteSegment> NoteSegments { get; set; }
    }
}
