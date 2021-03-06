﻿namespace MongoLibrary {

    public sealed class MongoDbOptions {
        
        public string Connection { get; set; }
        
        public string Database { get; set; }
        
        public string CoreLogCollection { get; set; }
        
        public string FeedbackCollection { get; set; }
        
        public string ClientLogCollection { get; set; }
        
        public string AccountLogCollection { get; set; }
        
        public string CooperationLogCollection { get; set; }
    }
}