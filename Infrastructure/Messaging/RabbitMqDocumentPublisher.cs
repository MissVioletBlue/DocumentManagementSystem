namespace Infrastructure.Messaging;

using System.Text;
using System.Text.Json;
using Application.Messaging;
using Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

public sealed class RabbitMqDocumentPublisher : IDocumentQueuePublisher
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqDocumentPublisher> _logger;

    public RabbitMqDocumentPublisher(
        IConnectionFactory connectionFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqDocumentPublisher> logger)
    {
        _connectionFactory = connectionFactory;
        _options = options.Value;
        _logger = logger;
    }

    public Task PublishDocumentUploadedAsync(DocumentUploadedMessage message, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue: _options.QueueName, durable: true, exclusive: false, autoDelete: false);

            var payload = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(payload);
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(exchange: string.Empty, routingKey: _options.QueueName, basicProperties: properties, body: body);
            _logger.LogInformation("Published document upload event for document {DocumentId}", message.DocumentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish document upload event for document {DocumentId}", message.DocumentId);
            throw new MessagingException("Unable to publish message to RabbitMQ.", ex);
        }

        return Task.CompletedTask;
    }
}