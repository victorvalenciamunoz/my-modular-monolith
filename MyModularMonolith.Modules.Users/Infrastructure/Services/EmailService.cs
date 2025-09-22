using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyModularMonolith.Modules.Users.Application.Services;
using System.Net.Mail;
using System.Text;

namespace MyModularMonolith.Modules.Users.Infrastructure.Services;

internal class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string firstName, string lastName, string? temporaryPassword = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var smtpHost = _configuration["Email:SmtpHost"] ?? "localhost";
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "25");
            var fromEmail = _configuration["Email:FromEmail"] ?? "noreply@mymodularmonolith.com";
            var fromName = _configuration["Email:FromName"] ?? "MyModularMonolith";

            using var client = new SmtpClient(smtpHost, smtpPort);
            client.EnableSsl = false; // Papercut doesn't use SSL

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = "¡Bienvenido a MyModularMonolith!",
                Body = BuildWelcomeEmailBody(firstName, lastName, temporaryPassword),
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8
            };

            mailMessage.To.Add(new MailAddress(toEmail));

            await client.SendMailAsync(mailMessage, cancellationToken);

            _logger.LogInformation("Welcome email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}", toEmail);
            throw;
        }
    }

    private static string BuildWelcomeEmailBody(string firstName, string lastName, string? temporaryPassword)
    {
        var bodyBuilder = new StringBuilder();
        bodyBuilder.AppendLine("<!DOCTYPE html>");
        bodyBuilder.AppendLine("<html>");
        bodyBuilder.AppendLine("<head>");
        bodyBuilder.AppendLine("<meta charset='utf-8'>");
        bodyBuilder.AppendLine("<title>¡Bienvenido!</title>");
        bodyBuilder.AppendLine("</head>");
        bodyBuilder.AppendLine("<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>");
        bodyBuilder.AppendLine("<div style='max-width: 600px; margin: 0 auto; padding: 20px;'>");

        bodyBuilder.AppendLine("<h1 style='color: #2c5aa0;'>¡Bienvenido a MyModularMonolith!</h1>");
        bodyBuilder.AppendLine($"<p>Hola <strong>{firstName} {lastName}</strong>,</p>");
        bodyBuilder.AppendLine("<p>¡Gracias por registrarte en nuestra plataforma! Tu cuenta ha sido creada exitosamente.</p>");

        if (!string.IsNullOrEmpty(temporaryPassword))
        {
            bodyBuilder.AppendLine("<div style='background-color: #f8f9fa; border: 1px solid #dee2e6; border-radius: 5px; padding: 15px; margin: 20px 0;'>");
            bodyBuilder.AppendLine("<h3 style='color: #dc3545; margin-top: 0;'>⚠️ Contraseña Temporal</h3>");
            bodyBuilder.AppendLine("<p>Se ha generado una <strong>contraseña temporal</strong> para tu cuenta:</p>");
            bodyBuilder.AppendLine($"<p style='font-family: monospace; font-size: 18px; background-color: #e9ecef; padding: 10px; border-radius: 3px; text-align: center;'><strong>{temporaryPassword}</strong></p>");
            bodyBuilder.AppendLine("<p style='color: #dc3545;'><strong>¡IMPORTANTE!</strong> Debes cambiar esta contraseña en tu primer inicio de sesión por motivos de seguridad.</p>");
            bodyBuilder.AppendLine("</div>");
        }

        bodyBuilder.AppendLine("<h3>Próximos pasos:</h3>");
        bodyBuilder.AppendLine("<ul>");
        bodyBuilder.AppendLine("<li>Inicia sesión en tu cuenta</li>");
        if (!string.IsNullOrEmpty(temporaryPassword))
        {
            bodyBuilder.AppendLine("<li>Cambia tu contraseña temporal por una segura</li>");
        }
        bodyBuilder.AppendLine("<li>Completa tu perfil</li>");
        bodyBuilder.AppendLine("<li>¡Comienza a usar la plataforma!</li>");
        bodyBuilder.AppendLine("</ul>");

        bodyBuilder.AppendLine("<p>Si tienes alguna pregunta o necesitas ayuda, no dudes en contactarnos.</p>");
        bodyBuilder.AppendLine("<p>¡Esperamos que disfrutes usando MyModularMonolith!</p>");

        bodyBuilder.AppendLine("<hr style='border: none; border-top: 1px solid #dee2e6; margin: 30px 0;'>");
        bodyBuilder.AppendLine("<p style='font-size: 12px; color: #6c757d;'>Este es un mensaje automático, por favor no respondas a este correo.</p>");

        bodyBuilder.AppendLine("</div>");
        bodyBuilder.AppendLine("</body>");
        bodyBuilder.AppendLine("</html>");

        return bodyBuilder.ToString();
    }
}
