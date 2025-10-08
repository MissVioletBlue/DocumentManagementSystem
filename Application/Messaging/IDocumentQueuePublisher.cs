namespace Application.Messaging;

public interface IDocumentQueuePublisher
{
    Task PublishDocumentUploadedAsync(DocumentUploadedMessage message, CancellationToken ct = default);
}