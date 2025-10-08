using Microsoft.Extensions.Logging;

namespace Application.Messaging;

public sealed class NoOpDocumentQueuePublisher(ILogger<NoOpDocumentQueuePublisher> logger) : IDocumentQueuePublisher
{
    public Task PublishDocumentUploadedAsync(DocumentUploadedMessage message, CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromCanceled(ct);

        logger.LogInformation("Skipping queue publish for document {DocumentId} in testing environment", message.DocumentId);
        return Task.CompletedTask;
    }
}
