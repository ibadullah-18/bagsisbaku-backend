using bagsisbaku.Application.Abstractions.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace bagsisbaku.Infrastructure.Email;

internal sealed class SmtpEmailSender : IEmailSender
{
    private readonly SmtpEmailSettings _settings;

    public SmtpEmailSender(
        SmtpEmailSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        settings.Validate();
        _settings = settings;
    }

    public async Task SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (string.IsNullOrWhiteSpace(message.ToEmail))
        {
            throw new ArgumentException(
                "Email alıcısı boş ola bilməz.",
                nameof(message));
        }

        if (string.IsNullOrWhiteSpace(message.Subject))
        {
            throw new ArgumentException(
                "Email başlığı boş ola bilməz.",
                nameof(message));
        }

        var mimeMessage = new MimeMessage();

        mimeMessage.From.Add(
            new MailboxAddress(
                _settings.FromName,
                _settings.FromEmail!));

        mimeMessage.To.Add(
            MailboxAddress.Parse(message.ToEmail));

        mimeMessage.Subject = message.Subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = message.HtmlBody,
            TextBody = message.TextBody
        };

        mimeMessage.Body = bodyBuilder.ToMessageBody();

        using var smtpClient = new SmtpClient();

        smtpClient.CheckCertificateRevocation = true;

        try
        {
            await smtpClient.ConnectAsync(
                _settings.Host!,
                _settings.Port,
                SecureSocketOptions.StartTls,
                cancellationToken);

            await smtpClient.AuthenticateAsync(
                _settings.Username!,
                _settings.Password!,
                cancellationToken);

            await smtpClient.SendAsync(
                mimeMessage,
                cancellationToken);
        }
        finally
        {
            if (smtpClient.IsConnected)
            {
                await smtpClient.DisconnectAsync(
                    true,
                    CancellationToken.None);
            }
        }
    }
}

