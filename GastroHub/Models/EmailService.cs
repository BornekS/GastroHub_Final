
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace GastroHub.Models;
public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void SendConfirmationEmail(string toEmail, string username)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress("GastroHub", _configuration["EmailSettings:SenderEmail"]));
        email.To.Add(new MailboxAddress(username, toEmail));
        email.Subject = "Potvrda registracije na GastroHub";

        // E-mail sadržaj
        email.Body = new TextPart("plain")
        {
            Text = $"Poštovani {username},\n\nHvala na registraciji na GastroHub!\n\nVaš račun je uspješno aktiviran.\n\nPozdrav,\nGastroHub tim"
        };

        using (var smtp = new SmtpClient())
        {
            smtp.Connect(_configuration["EmailSettings:SmtpServer"], int.Parse(_configuration["EmailSettings:SmtpPort"]), true);
            smtp.Authenticate(_configuration["EmailSettings:SmtpUsername"], _configuration["EmailSettings:SmtpPassword"]);
            smtp.Send(email);
            smtp.Disconnect(true);
        }
    }
}
