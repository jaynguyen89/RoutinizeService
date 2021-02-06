using Microsoft.AspNetCore.Http;

namespace MediaLibrary.ViewModels {

    public sealed class ImgUploadVM {
        
        public int TokenId { get; set; }
        
        public int AccountId { get; set; }
        
        public IFormFile UploadedFile { get; set; }
    }
}