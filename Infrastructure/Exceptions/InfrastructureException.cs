namespace Infrastructure.Exceptions;

public abstract class InfrastructureException(string message, Exception? innerException = null)
    : Exception(message, innerException);