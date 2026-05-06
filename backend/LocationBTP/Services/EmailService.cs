using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace LocationBTP.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task EnvoyerAsync(string destinataire, string sujet, string corps, byte[] pdfJointe = null, string nomFichier = null)
        {
            var settings = _config.GetSection("EmailSettings");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(settings["NomExpediteur"], settings["Email"]));
            message.To.Add(MailboxAddress.Parse(destinataire));
            message.Subject = sujet;

            var builder = new BodyBuilder();
            builder.HtmlBody = corps;

            if (pdfJointe != null && nomFichier != null)
                builder.Attachments.Add(nomFichier, pdfJointe, ContentType.Parse("application/pdf"));

            message.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(settings["Host"], int.Parse(settings["Port"]), SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(settings["Email"], settings["Password"]);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
        }
    }
}