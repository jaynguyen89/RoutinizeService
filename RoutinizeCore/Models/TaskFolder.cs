using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class TaskFolder
    {
        public TaskFolder()
        {
            FolderItems = new HashSet<FolderItem>();
        }

        public int Id { get; set; }
        public int CreatedById { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? ColorId { get; set; }
        public string FillColor { get; set; }
        public DateTime CreatedOn { get; set; }

        public virtual ColorPallete Color { get; set; }
        public virtual User CreatedBy { get; set; }
        public virtual ICollection<FolderItem> FolderItems { get; set; }
    }
}
