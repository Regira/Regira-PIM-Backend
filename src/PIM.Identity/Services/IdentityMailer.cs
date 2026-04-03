using Microsoft.AspNetCore.Identity.UI.Services;
using Regira.Office.Mail.Abstractions;
using Regira.Office.Mail.Models;

namespace PIM.Identity.Services;

public class IdentityMailer(IMailer mailer) : IEmailSender
{
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var msg = new MessageObject
        {
            From = new MailRecipient { Email = "admin@regira.com", DisplayName = "Regira PIM Admin" },
            To = { new MailRecipient { Email = email } },
            Subject = subject,
            Body = htmlMessage
        };
        var response = await mailer.Send(msg);
    }
}