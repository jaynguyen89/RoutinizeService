using System;
using System.Collections.Generic;

#nullable disable

namespace RoutinizeCore.Models
{
    public partial class AttachmentPermission
    {
        public AttachmentPermission()
        {
            Attachments = new HashSet<Attachment>();
        }

        public int Id { get; set; }
        public bool AllowViewToEveryone { get; set; }
        public string MembersToAllowView { get; set; }
        public bool AllowEditToEveryone { get; set; }
        public string MembersToAllowEdit { get; set; }
        public bool AllowDeleteToEveryone { get; set; }
        public string MembersToAllowDelete { get; set; }
        public bool AllowDownloadToEveryone { get; set; }
        public string MembersToAllowDownload { get; set; }

        public virtual ICollection<Attachment> Attachments { get; set; }
    }
}
