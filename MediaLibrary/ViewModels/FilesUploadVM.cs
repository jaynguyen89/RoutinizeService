using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace MediaLibrary.ViewModels {

    public class FilesUploadVM {
        
        public int AccountId { get; set; }
        
        public int TokenId { get; set; }
        
        public int ItemId { get; set; }
        
        public IFormFileCollection UploadedFiles { get; set; }
    }
}