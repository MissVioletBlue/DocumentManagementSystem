using Infrastructure.Messaging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace OcrWorker;

public interface IRabbitConnectionFactory
{
    IConnection CreateConnection();
}

public sealed class RabbitConnectionFactory(IOptions<RabbitMqOptions> options) : IRabbitConnectionFactory
{
    private readonly RabbitMqOptions _options = options.Value;

    public IConnection CreateConnection()
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            VirtualHost = _options.VirtualHost,
            UserName = _options.UserName,
            Password = _options.Password,
            DispatchConsumersAsync = true
        };

        return factory.CreateConnection();
    }
}