using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class ContentFolder
    {
        public ContentFolder()
        {
            FolderItems = new HashSet<FolderItem>();
        }

        public int Id { get; set; }
        public int CreatedById { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ColorTag { get; set; }
        public DateTime CreatedOn { get; set; }

        public virtual User CreatedBy { get; set; }
        public virtual ICollection<FolderItem> FolderItems { get; set; }
    }
}
