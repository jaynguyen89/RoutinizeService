namespace AssistantLibrary.Models {

    public class MailServerOptions {
        
        public string MailServerHost { get; set; }
        
        public string MailServerPort { get; set; }
        
        public string MailServerTls { get; set; }
        
        public string UseDefaultCredentials { get; set; }
        
        public string MailSenderAddress { get; set; }
        
        public string MailSenderPassword { get; set; }
        
        public string MailSenderName { get; set; }
    }
}