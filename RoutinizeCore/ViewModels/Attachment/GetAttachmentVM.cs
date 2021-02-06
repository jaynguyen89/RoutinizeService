using System;
using HelperLibrary.Shared;
using RoutinizeCore.ViewModels.User;

namespace RoutinizeCore.ViewModels.Attachment {

    public sealed class GetAttachmentVM {
        
        public int Id { get; set; }
        
        public byte Type { get; set; }
        
        public AtmPlace Place { get; set; }
        
        public AtmFile File { get; set; }
        
        public AtmAudit Audit { get; set; }
    }
    
    public sealed class AtmPlace {
        
        public AddressVM Address { get; set; }
        
        public PermissionVM Permission { get; set; }
    }

    public sealed class AtmFile {
        
        public string Name { get; set; }
        
        public string Url { get; set; }
        
        public bool IsHttp { get; set; }
        
        public PermissionVM Permission { get; set; }
    }

    public sealed class AtmAudit {
        
        public AtmAuthor Author { get; set; }
        
        public DateTime UploadedOn { get; set; }
    }

    public sealed class AtmAuthor {
        
        public int Id { get; set; }
        
        public string FullName { get; set; }
        
        public string Avatar { get; set; }
    }
}