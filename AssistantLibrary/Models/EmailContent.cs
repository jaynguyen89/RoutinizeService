using System.Collections.Generic;

namespace AssistantLibrary.Models {

    public sealed class EmailContent {
        
        //The below 2 params leave NULL to take default values from Azure AppConfig, otherwise, set your desired values
        public string SenderAddress { get; set; } = null;

        public string SenderName { get; set; } = null;

        public string ReceiverAddress { get; set; }

        public string ReceiverName { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public List<string> Attachments { get; set; } = null;
    }
}