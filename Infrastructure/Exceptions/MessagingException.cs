namespace Infrastructure.Exceptions;

public sealed class MessagingException(string message, Exception? innerException = null)
    : InfrastructureException(message, innerException);