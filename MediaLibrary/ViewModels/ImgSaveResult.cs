namespace MediaLibrary.ViewModels {

    public sealed class ImgSaveResult {
        
        public bool Error { get; set; }
        
        public string ErrorMessage { get; set; }
        
        public FileSaveResult Result { get; set; }
    }
}