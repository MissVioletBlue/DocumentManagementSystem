namespace Infrastructure.Documents;

using Application.Documents;
using Domain;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;

public sealed class DocumentRepository : IDocumentRepository
{
    private readonly DocumentDbContext _db;
    private readonly ILogger<DocumentRepository> _logger;

    public DocumentRepository(DocumentDbContext db, ILogger<DocumentRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<DocumentModel?> GetAsync(int id, CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching document with id {DocumentId}", id);
        return await _db.Documents.AsNoTracking().FirstOrDefaultAsync(d => d.UniqueIdentifier == id, ct);
    }
    public async Task<IReadOnlyList<DocumentModel>> SearchAsync(string? q, CancellationToken ct = default)
    {
        _logger.LogDebug("Searching documents with query {Query}", q);
        var query = _db.Documents.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(d => d.DocumentTitle!.Contains(q));
        return await query.OrderBy(d => d.UniqueIdentifier).ToListAsync(ct);
    }

    public async Task<DocumentModel> AddAsync(DocumentModel doc, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Creating document {Title}", doc.DocumentTitle);
            await _db.Documents.AddAsync(doc, ct);
            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("Document {DocumentId} created", doc.UniqueIdentifier);
            return doc;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to create document {Title}", doc.DocumentTitle);
            throw new DocumentRepositoryException("Unable to create document.", ex);
        }
    }

    public async Task<bool> UpdateAsync(DocumentModel doc, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Updating document {DocumentId}", doc.UniqueIdentifier);
            _db.Documents.Update(doc);
            var affected = await _db.SaveChangesAsync(ct) > 0;
            _logger.LogInformation("Document {DocumentId} update {Status}", doc.UniqueIdentifier, affected ? "succeeded" : "had no effect");
            return affected;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict while updating document {DocumentId}", doc.UniqueIdentifier);
            throw new DocumentRepositoryException("The document was updated by another process.", ex);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to update document {DocumentId}", doc.UniqueIdentifier);
            throw new DocumentRepositoryException("Unable to update document.", ex);
        }
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Deleting document {DocumentId}", id);
            var entity = await _db.Documents.FindAsync(new object?[] { id }, ct);
            if (entity is null)
            {
                _logger.LogWarning("Document {DocumentId} not found for deletion", id);
                return false;
            }
            _db.Documents.Remove(entity);
            var affected = await _db.SaveChangesAsync(ct) > 0;
            _logger.LogInformation("Document {DocumentId} deletion {Status}", id, affected ? "succeeded" : "had no effect");
            return affected;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to delete document {DocumentId}", id);
            throw new DocumentRepositoryException("Unable to delete document.", ex);
        }
    }
}