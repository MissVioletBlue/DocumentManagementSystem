namespace Application.Exceptions;

public abstract class ApplicationLayerException(string message, Exception? innerException = null)
    : Exception(message, innerException);