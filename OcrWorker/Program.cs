using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OcrWorker;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<RabbitMqOptions>(context.Configuration.GetSection("RabbitMq"));
        services.AddSingleton<IRabbitConnectionFactory, RabbitConnectionFactory>();
        services.AddHostedService<QueueWorker>();
    })
    .Build()
    .RunAsync();