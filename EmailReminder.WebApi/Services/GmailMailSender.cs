using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using EmailReminder.Shared.Models;
using Mailjet.Client;

namespace EmailReminder.WebApi.Services
{
    public class GmailMailSender : IMailSender
    {
        private readonly IAuthenticationService _authenticationService;
        public GmailMailSender(
            IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public void SendMail(string subject, string body, string to)
        {
            using (var client = new SmtpClient("smtp.gmail.com", 587))
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential("YOUR_EMAIL", "YOUR_PASSWORD");
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.EnableSsl = true;

                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress("YOUR_EMAIL");
                mailMessage.To.Add(to);
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                client.Send(mailMessage);
            }
        }

        public async void SendReminderAsync(Reminder reminder)
        {
            var isUserConfirmed = await _authenticationService.IsUserConfirmedAsync(reminder.EmailAddress);

            if (!isUserConfirmed)
            {
                return;
            }

            SendMail(
                $"Your reminder for {reminder.DateTime.ToShortDateString()}",
                reminder.Message,
                reminder.EmailAddress);
        }

        public void SendVerification(string email, string token)
        {
            SendMail(
                $"Your email confirmation",
                BuildLink("confirm", email, token),
                email);
        }

        public void SendLoginToken(string email, string token)
        {
            SendMail(
            $"Your login token",
            BuildLink("login", email, token),
            email);
        }

        private string BuildLink(string page, string email, string token)
        {
            var sb = new StringBuilder();

            sb
                .Append($"https://localhost:44337/{page}?email=")
                .Append(HttpUtility.UrlEncode(email))
                .Append("&token=")
                .Append(HttpUtility.UrlEncode(token));

            return sb.ToString();
        }
    }
}
