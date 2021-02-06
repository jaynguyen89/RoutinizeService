using Microsoft.AspNetCore.Http;

namespace RoutinizeCore.ViewModels.Attachment {

    public class CoverUploadVM {
        
        public int TokenId { get; set; }
        
        public int TodoId { get; set; }
        
        public IFormFile UploadedFile { get; set; }
    }
}