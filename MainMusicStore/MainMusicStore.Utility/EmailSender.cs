using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;

namespace MainMusicStore.Utility
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailOptions _emailOptions;

        public EmailSender(IOptions<EmailOptions> emailOptions)
        {
            _emailOptions = emailOptions.Value;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Execute(_emailOptions.SendGridApiKey,subject,htmlMessage,email);
        }

        private Task Execute(string sendGridApiKey,string subject, string htmlMessage,string email)
        {
          
            var client = new SendGridClient(sendGridApiKey);
            var from = new EmailAddress("ekiznurettin@gmail.com", "Example User");
            var to = new EmailAddress(email, "Example User");
            var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlMessage);
            return  client.SendEmailAsync(msg);
        }
    }
}
