using HelperLibrary.Shared;
using Microsoft.AspNetCore.Http;

namespace RoutinizeCore.ViewModels.Attachment {

    public sealed class CoverImageVM {
        
        public int TokenId { get; set; }
        
        public int ItemId { get; set; }
        
        public string ItemType { get; set; }

        public string TaskType { get; set; } = SharedConstants.TaskUpdate;
        
        public IFormFile UploadedFile { get; set; }
    }
}