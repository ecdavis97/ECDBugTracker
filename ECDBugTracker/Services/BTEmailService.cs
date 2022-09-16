using ECDBugTracker.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;


namespace ECDBugTracker.Services
{
    public class BTEmailService : IEmailSender
    {
        private readonly MailSettings _mailSettings;
        public BTEmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            //configuration setup
            string? emailSender = _mailSettings.EmailAddress ?? Environment.GetEnvironmentVariable("Email");
            string? host = _mailSettings.EmailHost ?? Environment.GetEnvironmentVariable("Host");
            int port = _mailSettings.EmailPort != 0 ? _mailSettings.EmailPort : int.Parse(Environment.GetEnvironmentVariable("Port")!);
            string password = _mailSettings.EmailPassword ?? Environment.GetEnvironmentVariable("Password")!;

            //email setup
            MimeMessage newEmail = new();

            //add all email addresses to the "TO" for the email
            newEmail.Sender = MailboxAddress.Parse(email);
            newEmail.To.Add(MailboxAddress.Parse(email));
            newEmail.Subject = subject;

            //add the body to the email 
            BodyBuilder emailBody = new();
            emailBody.HtmlBody = htmlMessage;
            newEmail.Body = emailBody.ToMessageBody();

            //email send
            try
            {
                //send the email
                using SmtpClient smtpClient = new();

                await smtpClient.ConnectAsync(host, port, SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(emailSender, password);

                await smtpClient.SendAsync(newEmail);
                await smtpClient.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                throw;
            }
        }
    }
}