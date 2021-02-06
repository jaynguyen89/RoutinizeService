using Newtonsoft.Json;
using RoutinizeCore.ViewModels.Attachment;

namespace RoutinizeCore.Models {

    public partial class AttachmentPermission {

        public static implicit operator AttachmentPermission(AttachmentPermissionVM atmPermissions) {
            return new() {
                AllowViewToEveryone = atmPermissions.Permissions.AllowAnyoneView,
                MembersToAllowView = atmPermissions.Permissions.MembersCanView.Length == 0 ? null : JsonConvert.SerializeObject(atmPermissions.Permissions.MembersCanView),
                AllowEditToEveryone = atmPermissions.Permissions.AllowAnyoneEdit,
                MembersToAllowEdit = atmPermissions.Permissions.MembersCanEdit.Length == 0 ? null : JsonConvert.SerializeObject(atmPermissions.Permissions.MembersCanEdit),
                AllowDeleteToEveryone = atmPermissions.Permissions.AllowAnyoneDelete,
                MembersToAllowDelete = atmPermissions.Permissions.MembersCanDelete.Length == 0 ? null : JsonConvert.SerializeObject(atmPermissions.Permissions.MembersCanDelete),
                AllowDownloadToEveryone = atmPermissions.Permissions.AllowAnyoneDownload,
                MembersToAllowDownload = atmPermissions.Permissions.MembersCanDownload.Length == 0 ? null : JsonConvert.SerializeObject(atmPermissions.Permissions.MembersCanDownload),
            };
        }
    }
}