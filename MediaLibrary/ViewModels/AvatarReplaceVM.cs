using Microsoft.AspNetCore.Http;

namespace MediaLibrary.ViewModels {

    public class AvatarReplaceVM {
        
        public int TokenId { get; set; }
        
        public int AccountId { get; set; }
        
        public IFormFile FileToSave { get; set; }
        
        public string CurrentAvatar { get; set; }
    }
}