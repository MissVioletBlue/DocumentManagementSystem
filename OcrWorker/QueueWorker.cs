using System.Text;
using System.Threading;
using Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OcrWorker;

public sealed class QueueWorker : BackgroundService
{
    private readonly ILogger<QueueWorker> _logger;
    private readonly IRabbitConnectionFactory _connectionFactory;
    private readonly RabbitMqOptions _options;
    private IConnection? _connection;
    private IModel? _channel;

    public QueueWorker(
        ILogger<QueueWorker> logger,
        IRabbitConnectionFactory connectionFactory,
        IOptions<RabbitMqOptions> options)
    {
        _logger = logger;
        _connectionFactory = connectionFactory;
        _options = options.Value;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting OCR worker listening to queue {Queue}", _options.QueueName);
        stoppingToken.Register(DisposeResources);

        _connection = _connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: _options.QueueName, durable: true, exclusive: false, autoDelete: false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation("Received document message: {Message}", message);
                await Task.Yield(); // placeholder for OCR work
                _channel?.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process message {DeliveryTag}", ea.DeliveryTag);
                if (_channel?.IsOpen == true)
                {
                    _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
                }
            }
        };

        _channel.BasicConsume(queue: _options.QueueName, autoAck: false, consumer: consumer);

        return Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private void DisposeResources()
    {
        try
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error disposing RabbitMQ resources");
        }
    }

    public override void Dispose()
    {
        DisposeResources();
        base.Dispose();
    }
}