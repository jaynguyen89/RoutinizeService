using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AssistantLibrary.Models;

namespace AssistantLibrary.Interfaces {

    public interface IEmailSenderService {

        /// <summary>
        /// Sends an email with the content of type EmailContent.
        /// </summary>
        Task<bool> SendEmailSingle([NotNull] EmailContent emailContent);

        /// <summary>
        /// Sends multiple emails from List<EmailContent>. Returns KeyValuePair:
        /// Key==True if all emails are sent successfully.
        /// Key==False if at least 1 email was failed to send, Value is the List of failed-to-send emails.
        /// </summary>
        Task<KeyValuePair<bool, List<EmailContent>>> SendEmailsMultiple([NotNull] IEnumerable<EmailContent> emailContents);
    }
}