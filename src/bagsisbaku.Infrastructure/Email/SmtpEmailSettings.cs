namespace bagsisbaku.Infrastructure.Email;

public sealed record SmtpEmailSettings(
    string? Host,
    int Port,
    string? Username,
    string? Password,
    string? FromEmail,
    string? FromName)
{
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Host))
        {
            throw new InvalidOperationException(
                "Email:Smtp:Host konfiqurasiyası tapılmadı.");
        }

        if (Port is < 1 or > 65535)
        {
            throw new InvalidOperationException(
                "Email:Smtp:Port düzgün deyil.");
        }

        if (string.IsNullOrWhiteSpace(Username))
        {
            throw new InvalidOperationException(
                "Email:Smtp:Username konfiqurasiyası tapılmadı.");
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            throw new InvalidOperationException(
                "Email:Smtp:Password konfiqurasiyası tapılmadı.");
        }

        if (string.IsNullOrWhiteSpace(FromEmail))
        {
            throw new InvalidOperationException(
                "Email:Smtp:FromEmail konfiqurasiyası tapılmadı.");
        }

        if (string.IsNullOrWhiteSpace(FromName))
        {
            throw new InvalidOperationException(
                "Email:Smtp:FromName konfiqurasiyası tapılmadı.");
        }
    }
}
