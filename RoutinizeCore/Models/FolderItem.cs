using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class FolderItem
    {
        public int Id { get; set; }
        public int FolderId { get; set; }
        public int ItemId { get; set; }
        public string ItemType { get; set; }
        public DateTime AddedOn { get; set; }

        public virtual TaskFolder Folder { get; set; }
    }
}
