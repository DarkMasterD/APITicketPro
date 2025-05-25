using System.Net.Mail;
using System.Net;

namespace APITicketPro.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task EnviarNotificacion(string paraCorreo, string asunto, string cuerpo)
        {
            var remitente = _config["EmailSettings:Remitente"];
            var nombre = _config["EmailSettings:NombreRemitente"];
            var password = _config["EmailSettings:Password"];
            var host = _config["EmailSettings:Host"];
            var port = int.Parse(_config["EmailSettings:Port"]);

            var mensaje = new MailMessage
            {
                From = new MailAddress(remitente, nombre),
                Subject = asunto,
                Body = cuerpo,
                IsBodyHtml = true
            };

            mensaje.To.Add(paraCorreo);

            using var smtp = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(remitente, password),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mensaje);
        }
    }
}
