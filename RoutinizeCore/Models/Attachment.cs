using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class Attachment
    {
        public Attachment()
        {
            Projects = new HashSet<Project>();
            Teams = new HashSet<Team>();
        }

        public int Id { get; set; }
        public int ItemId { get; set; }
        public string ItemType { get; set; }
        public byte AttachmentType { get; set; }
        public string AttachmentName { get; set; }
        public string AttachmentUrl { get; set; }

        public virtual ICollection<Project> Projects { get; set; }
        public virtual ICollection<Team> Teams { get; set; }
    }
}
