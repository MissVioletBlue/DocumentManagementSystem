namespace Infrastructure.Documents;

using Application.Documents;
using Domain;
using Microsoft.EntityFrameworkCore;

public sealed class DocumentRepository : IDocumentRepository
{
    private readonly DocumentDbContext _db;
    public DocumentRepository(DocumentDbContext db) => _db = db;

    public Task<DocumentModel?> GetAsync(int id, CancellationToken ct = default) =>
        _db.Documents.AsNoTracking().FirstOrDefaultAsync(d => d.UniqueIdentifier == id, ct);

    public async Task<IReadOnlyList<DocumentModel>> SearchAsync(string? q, CancellationToken ct = default)
    {
        var query = _db.Documents.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(d => d.DocumentTitle!.Contains(q));
        return await query.OrderBy(d => d.UniqueIdentifier).ToListAsync(ct);
    }

    public async Task<DocumentModel> AddAsync(DocumentModel doc, CancellationToken ct = default)
    {
        _db.Documents.Add(doc);
        await _db.SaveChangesAsync(ct);
        return doc;
    }

    public async Task<bool> UpdateAsync(DocumentModel doc, CancellationToken ct = default)
    {
        _db.Documents.Update(doc);
        return await _db.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Documents.FindAsync(new object?[] { id }, ct);
        if (entity is null) return false;

        _db.Documents.Remove(entity);
        return await _db.SaveChangesAsync(ct) > 0;
    }
}