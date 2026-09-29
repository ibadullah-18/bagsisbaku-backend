using System.Collections.Concurrent;
using bagsisbaku.Application.Abstractions.Email;

namespace bagsisbaku.IntegrationTests;

internal sealed class TestEmailSender : IEmailSender
{
    private readonly ConcurrentQueue<EmailMessage>
        _messages = new();

    public IReadOnlyCollection<EmailMessage>
        Messages =>
            _messages.ToArray();

    public Task SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            message);

        cancellationToken
            .ThrowIfCancellationRequested();

        _messages.Enqueue(
            message);

        return Task.CompletedTask;
    }

    public void Clear()
    {
        while (_messages.TryDequeue(out _))
        {
        }
    }
}