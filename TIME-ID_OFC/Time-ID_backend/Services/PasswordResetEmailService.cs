using System.Net;
using System.Net.Mail;
using TimeId.Operations;

public sealed class PasswordResetEmailService(IConfiguration configuration)
{
    public async Task SendAsync(string recipient, string code)
    {
        IConfigurationSection settings = configuration.GetSection("Smtp");
        string? host = settings["Host"];
        string? from = settings["From"];
        string? password = settings["Password"];
        string? username = settings["Username"];
        int port = settings.GetValue<int?>("Port") ?? 587;
        bool useSsl = settings.GetValue<bool?>("UseSsl") ?? true;

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
            throw new DomainError(503, "O envio de e-mails ainda não está configurado.");

        using var message = new MailMessage
        {
            From = new MailAddress(from, "TIMEID"),
            Subject = "Código para redefinir sua senha — TIMEID",
            Body = $"""
                <div style="font-family:Arial,sans-serif;color:#063b2b;max-width:520px;margin:auto;padding:28px;border:1px solid #b9ead4;border-radius:16px">
                  <h1 style="margin:0 0 12px;color:#008c55">TIMEID</h1>
                  <p>Use o código abaixo para redefinir sua senha:</p>
                  <p style="margin:24px 0;font-size:32px;font-weight:bold;letter-spacing:9px;color:#008c55">{code}</p>
                  <p>O código expira em 15 minutos e só pode ser usado uma vez.</p>
                  <p>Se você não solicitou essa alteração, ignore este e-mail.</p>
                </div>
                """,
            IsBodyHtml = true,
        };
        message.To.Add(recipient);

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = useSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
        };
        if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            client.Credentials = new NetworkCredential(username, password);

        try
        {
            await client.SendMailAsync(message);
        }
        catch (SmtpException)
        {
            throw new DomainError(503, "Não foi possível enviar o e-mail agora. Tente novamente em alguns minutos.");
        }
    }
}
