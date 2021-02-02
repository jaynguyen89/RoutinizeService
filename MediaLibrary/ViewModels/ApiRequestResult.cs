namespace MediaLibrary.ViewModels {

    public sealed class ApiRequestResult {
        
        public bool Error { get; set; }
        
        public string ErrorMessage { get; set; }
        
        public ResultVM Result { get; set; }

        public sealed class ResultVM {
        
            public string Name { get; set; }
        
            public string Location { get; set; }
        }
    }
}