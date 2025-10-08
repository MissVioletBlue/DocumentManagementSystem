using Application.Documents;
using Application.Messaging;
using Domain;
using Domain.Exceptions;
using Infrastructure;
using Infrastructure.Documents;
using Infrastructure.Exceptions;
using Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var useInMemory = builder.Configuration.GetValue<bool>("UseInMemoryTestDatabase");
if (!useInMemory && string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("A connection string named 'DefaultConnection' was not provided.");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler(options =>
{
    options.ExceptionHandler = async context =>
    {
        var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>()?.Error;
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        if (exception is not null)
            logger.LogError(exception, "Unhandled exception while processing {Path}", context.Request.Path);

        var (statusCode, title) = exception switch
        {
            Application.Exceptions.ApplicationLayerException => (StatusCodes.Status400BadRequest, "Application error"),
            InfrastructureException => (StatusCodes.Status503ServiceUnavailable, "Infrastructure error"),
            DomainException => (StatusCodes.Status422UnprocessableEntity, "Domain validation error"),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected error")
        };

        var problem = Results.Problem(
            title: title,
            detail: exception?.Message,
            statusCode: statusCode,
            instance: context.Request.Path);

        context.Response.StatusCode = statusCode;
        await problem.ExecuteAsync(context);
    };
});

if (useInMemory)
{
    builder.Services.AddDbContext<DocumentDbContext>(opt =>
        opt.UseInMemoryDatabase("tests"));
}
else
{
    builder.Services.AddDbContext<DocumentDbContext>(opt =>
        opt.UseNpgsql(connectionString));
}

builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();

if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddSingleton<IDocumentQueuePublisher, NoOpDocumentQueuePublisher>();
}
else
{
    builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));
    builder.Services.AddSingleton<IConnectionFactory>(sp =>
    {
        var options = sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
        if (string.IsNullOrWhiteSpace(options.HostName))
            throw new InvalidOperationException("RabbitMQ configuration is missing the host name.");

        return new ConnectionFactory
        {
            HostName = options.HostName,
            Port = options.Port,
            VirtualHost = options.VirtualHost,
            UserName = options.UserName,
            Password = options.Password,
            DispatchConsumersAsync = true
        };
    });
    builder.Services.AddSingleton<IDocumentQueuePublisher, RabbitMqDocumentPublisher>();
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DocumentDbContext>();
    if (db.Database.IsRelational())
        db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


var healthHandler = () => Results.Ok(new { status = "Healthy", time = DateTime.UtcNow });
app.MapGet("/health", healthHandler);
app.MapGet("/api/health", healthHandler).ExcludeFromDescription();

app.MapGet("/api/documents/{id:int}", async (int id, IDocumentRepository repo, CancellationToken ct) =>
{
    var doc = await repo.GetAsync(id, ct);
    return doc is null ? Results.NotFound() : Results.Ok(ToDto(doc));
})
.WithName("Documents_GetById");

app.MapGet("/api/documents", async (string? q, IDocumentRepository repo, CancellationToken ct) =>
{
    var list = await repo.SearchAsync(q, ct);
    return Results.Ok(list.Select(ToDto));
})
.WithName("Documents_Search");

app.MapPost("/api/documents", async (CreateDocumentDto dto, IDocumentRepository repo, IDocumentQueuePublisher queue, ILogger<Program> logger, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(dto.DocumentTitle))
        return Results.BadRequest("DocumentTitle is required.");
    var model = new DocumentModel
    {
        DocumentTitle = dto.DocumentTitle,
        DocumentLocation = dto.DocumentLocation,
        DocumentAuthor = dto.DocumentAuthor,
        DocumentTags = dto.DocumentTags
    };

    var created = await repo.AddAsync(model, ct);
    await queue.PublishDocumentUploadedAsync(
        new DocumentUploadedMessage(created.UniqueIdentifier, created.DocumentTitle!, created.DocumentLocation),
        ct);
    logger.LogInformation("Document {DocumentId} queued for OCR", created.UniqueIdentifier);
    return Results.Created($"/api/documents/{created.UniqueIdentifier}", ToDto(created));
})
.WithName("Documents_Create");

app.MapPut("/api/documents/{id:int}", async (int id, UpdateDocumentDto dto, IDocumentRepository repo, CancellationToken ct) =>
{
    var existing = await repo.GetAsync(id, ct);
    if (existing is null) return Results.NotFound();

    existing.DocumentTitle    = dto.DocumentTitle;
    existing.DocumentLocation = dto.DocumentLocation;
    existing.DocumentAuthor   = dto.DocumentAuthor;
    existing.DocumentTags     = dto.DocumentTags;

    var ok = await repo.UpdateAsync(existing, ct);
    return ok ? Results.NoContent() : Results.StatusCode(500);
})
.WithName("Documents_Update");

app.MapDelete("/api/documents/{id:int}", async (int id, IDocumentRepository repo, CancellationToken ct) =>
    (await repo.DeleteAsync(id, ct)) ? Results.NoContent() : Results.NotFound())
.WithName("Documents_Delete");

app.Run();

// mapper helper
static DocumentDto ToDto(DocumentModel m) =>
    new(m.UniqueIdentifier, m.DocumentTitle!, m.DocumentLocation, m.DocumentAuthor, m.DocumentTags);

public partial class Program { }
