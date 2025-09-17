using Application.Documents;
using Domain;
using Infrastructure;
using Infrastructure.Documents;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("A connection string named 'DefaultConnection' was not provided.");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DocumentDbContext>(opt => opt.UseNpgsql(connectionString));
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DocumentDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapGet("/health", () => Results.Ok(new { status = "Healthy", time = DateTime.UtcNow }));

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

app.MapPost("/api/documents", async (CreateDocumentDto dto, IDocumentRepository repo, CancellationToken ct) =>
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
    return Results.Created($"/api/documents/{created.UniqueIdentifier}", ToDto(created));
})
.WithName("Documents_Create");

app.MapPut("/api/documents/{id:int}", async (int id, UpdateDocumentDto dto, IDocumentRepository repo, CancellationToken ct) =>
{
    var existing = await repo.GetAsync(id, ct);
    if (existing is null) return Results.NotFound();

    existing.DocumentTitle   = dto.DocumentTitle;
    existing.DocumentLocation= dto.DocumentLocation;
    existing.DocumentAuthor  = dto.DocumentAuthor;
    existing.DocumentTags    = dto.DocumentTags;

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
