namespace MediaLibrary.ViewModels {

    public sealed class AtmSaveResult {
        
        public bool Error { get; set; }
        
        public string ErrorMessage { get; set; }
        
        public AtmResultVM Result { get; set; }

        public sealed class AtmResultVM {
        
            public FileSaveResult[] Saves { get; set; }
        
            public string[] Fails { get; set; }
            
            public string[] Oversizes { get; set; }
            
            public string[] Unknowns { get; set; }
        }
    }
}