using EmailReminder.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmailReminder.WebApi.Services
{
    public interface IMailSender
    {
        void SendReminderAsync(Reminder reminder);
        void SendVerification(string email, string token);
        void SendLoginToken(string email, string token);
    }
}
