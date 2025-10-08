namespace Application.Messaging;

public sealed record DocumentUploadedMessage(int DocumentId, string DocumentTitle, string? DocumentLocation);