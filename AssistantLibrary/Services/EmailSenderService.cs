using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using AssistantLibrary.Interfaces;
using AssistantLibrary.Models;
using HelperLibrary.Shared;
using Microsoft.Extensions.Options;

namespace AssistantLibrary.Services {

    public sealed class EmailSenderService : IEmailSenderService {

        private readonly SmtpClient _smtpClient;
        private MailMessage _message;

        private string _senderAddress { get; set; }

        private string _senderName { get; set; }

        public EmailSenderService(IOptions<MailServerOptions> options) {
            _smtpClient = new SmtpClient {
                Host = options.Value.MailServerHost,
                Port = int.Parse(options.Value.MailServerPort),
                EnableSsl = bool.Parse(options.Value.MailServerTls),
                UseDefaultCredentials = bool.Parse(options.Value.UseDefaultCredentials),
                Credentials = new NetworkCredential {
                    UserName = options.Value.MailSenderAddress,
                    Password = options.Value.MailSenderPassword
                }
            };

            _senderAddress = options.Value.MailSenderAddress;
            _senderName = options.Value.MailSenderName;
        }

        public async Task<bool> SendEmailSingle([NotNull] EmailContent emailContent) {
            try {
                ComposeEmail(emailContent);
                await _smtpClient.SendMailAsync(_message);

                return true;
            }
            catch (ArgumentNullException) {
                return false;
            }
        }

        public async Task<KeyValuePair<bool, List<EmailContent>>> SendEmailsMultiple([NotNull] IEnumerable<EmailContent> emailContents) {
            var emailsFailedToSend = new List<EmailContent>();
            
            foreach (var emailContent in emailContents) {
                try {
                    ComposeEmail(emailContent);
                    await _smtpClient.SendMailAsync(_message);
                }
                catch (ArgumentNullException) {
                    emailsFailedToSend.Add(emailContent);
                }
            }

            return emailsFailedToSend.Count == 0
                ? new KeyValuePair<bool, List<EmailContent>>(true, null)
                : new KeyValuePair<bool, List<EmailContent>>(false, emailsFailedToSend);
        }
        
        private void ComposeEmail([NotNull] EmailContent message) {
            _message = new MailMessage {
                From = new MailAddress(
                    string.IsNullOrEmpty(message.SenderAddress) ? _senderAddress : message.SenderAddress,
                    string.IsNullOrEmpty(message.SenderName) ? _senderName : message.SenderName
                )
            };
            
            _message.To.Add(new MailAddress(message.ReceiverAddress, message.ReceiverName));
            _message.Subject = message.Subject;

            _message.IsBodyHtml = true;
            _message.Body = message.Body;

            if (message.Attachments != null)
                foreach (var attachment in message.Attachments)
                    _message.Attachments.Add(new Attachment(attachment));
        }
    }
}