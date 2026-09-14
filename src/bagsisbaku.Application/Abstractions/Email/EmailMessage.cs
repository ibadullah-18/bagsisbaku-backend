namespace bagsisbaku.Application.Abstractions.Email;

public sealed record EmailMessage(
    string ToEmail,
    string Subject,
    string HtmlBody,
    string TextBody);
