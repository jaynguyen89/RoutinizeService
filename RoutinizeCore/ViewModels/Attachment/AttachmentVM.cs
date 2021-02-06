using HelperLibrary.Shared;
using Microsoft.AspNetCore.Http;

namespace RoutinizeCore.ViewModels.Attachment {

    public sealed class AttachmentVM {
        
        public int? AddressId { get; set; }
        
        //[JsonConverter(typeof(StringEnumConverter))]
        public SharedEnums.AttachmentTypes FileType { get; set; }
        
        public string FileName { get; set; }
        
        public string FileUrl { get; set; }
    }
}