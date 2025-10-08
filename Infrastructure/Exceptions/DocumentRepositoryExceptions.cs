namespace Infrastructure.Exceptions;

public sealed class DocumentRepositoryException(string message, Exception? innerException = null)
    : InfrastructureException(message, innerException);