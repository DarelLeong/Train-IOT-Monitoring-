namespace ESD_Project.Models
{
    using ESD_Project.Services;
    using System.Net;
    using System.Net.Mail;

    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _cfg;
        public SmtpEmailSender(IConfiguration cfg) => _cfg = cfg;

        public Task SendEmailAsync(string to, string subject, string body)
        {
            var msg = new MailMessage(
                _cfg["AlertEmail:From"], to, subject, body)
            { IsBodyHtml = false };
            var client = new SmtpClient(_cfg["AlertEmail:SmtpHost"], int.Parse(_cfg["AlertEmail:SmtpPort"]))
            {
                Credentials = new NetworkCredential(_cfg["AlertEmail:Username"], _cfg["AlertEmail:Password"]),
                EnableSsl = bool.Parse(_cfg["AlertEmail:UseSsl"] ?? "true")
            };
            return client.SendMailAsync(msg);
        }
    }
}
