using System.Collections.Generic;
using System.Threading.Tasks;
using AssistantLibrary.Interfaces;
using AssistantLibrary.Models;

namespace AssistantLibrary.Services {

    public sealed class EmailSenderService : IEmailSenderService {

        public Task<bool> SendEmailSingle(EmailContent emailContent) {
            throw new System.NotImplementedException();
        }

        public Task<KeyValuePair<bool, List<EmailContent>>> SendEmailsMultiple(List<EmailContent> emailContents) {
            throw new System.NotImplementedException();
        }
    }
}