using System.Collections.Generic;
using System.Threading.Tasks;
using AssistantLibrary.Models;

namespace AssistantLibrary.Interfaces {

    public interface IEmailSenderService {

        Task<bool> SendEmailSingle(EmailContent emailContent);

        Task<KeyValuePair<bool, List<EmailContent>>> SendEmailsMultiple(List<EmailContent> emailContents);
    }
}