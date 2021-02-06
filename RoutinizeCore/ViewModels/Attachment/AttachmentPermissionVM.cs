using System;
using HelperLibrary;
using Newtonsoft.Json;
using RoutinizeCore.Models;

namespace RoutinizeCore.ViewModels.Attachment {

    public class AttachmentPermissionVM {
        
        public int AttachmentId { get; set; }
        
        public PermissionVM Permissions { get; set; }
    }

    public sealed class PermissionVM {
        
        public bool AllowAnyoneView { get; set; }
        
        public int[] MembersCanView { get; set; }
        
        public bool AllowAnyoneEdit { get; set; }
        
        public int[] MembersCanEdit { get; set; }
        
        public bool AllowAnyoneDelete { get; set; }
        
        public int[] MembersCanDelete { get; set; }
        
        public bool AllowAnyoneDownload { get; set; }
        
        public int[] MembersCanDownload { get; set; }

        public static implicit operator PermissionVM(AttachmentPermission permission) {
            return permission == null
                ? null
                : new PermissionVM {
                    AllowAnyoneView = permission.AllowViewToEveryone,
                    MembersCanView = Helpers.IsProperString(permission.MembersToAllowView)
                        ? JsonConvert.DeserializeObject<int[]>(permission.MembersToAllowView)
                        : Array.Empty<int>(),
                    AllowAnyoneEdit = permission.AllowEditToEveryone,
                    MembersCanEdit = Helpers.IsProperString(permission.MembersToAllowEdit)
                        ? JsonConvert.DeserializeObject<int[]>(permission.MembersToAllowEdit)
                        : Array.Empty<int>(),
                    AllowAnyoneDelete = permission.AllowDeleteToEveryone,
                    MembersCanDelete = Helpers.IsProperString(permission.MembersToAllowDelete)
                        ? JsonConvert.DeserializeObject<int[]>(permission.MembersToAllowDelete)
                        : Array.Empty<int>(),
                    AllowAnyoneDownload = permission.AllowDownloadToEveryone,
                    MembersCanDownload = Helpers.IsProperString(permission.MembersToAllowDownload)
                        ? JsonConvert.DeserializeObject<int[]>(permission.MembersToAllowDownload)
                        : Array.Empty<int>()
                };
        }
    }
}