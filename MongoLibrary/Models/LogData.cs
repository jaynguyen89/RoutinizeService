﻿using System;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoLibrary.Models {

    public class LogData {
        
        public string Controller { get; set; }
        
        public string Action { get; set; }
        
        public string ParamData { get; set; }
        
        public string BriefInformation { get; set; }
        
        public string DetailedInformation { get; set; }
        
        public string Severity { get; set; }
        
        [BsonDateTimeOptions]
        public DateTime? RecordedOn { get; set; }
    }
}