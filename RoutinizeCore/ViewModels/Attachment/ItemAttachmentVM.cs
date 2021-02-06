using Microsoft.AspNetCore.Http;

namespace RoutinizeCore.ViewModels.Attachment {

    public sealed class ItemAttachmentVM {
        
        public int ItemId { get; set; }
        
        public string ItemType { get; set; }

        public int UploadedById { get; set; }
        
        public int TokenId { get; set; }
        
        //For attaching Location and URLs, original file name must be sent separately
        public AttachmentVM[] Attachments { get; set; }
        
        //For uploading files, original file name must be kept as is in the request.
        public IFormFileCollection UploadedFiles { get; set; }
    }
}