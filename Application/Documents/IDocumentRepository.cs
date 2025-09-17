namespace Application.Documents;

using Domain;

public interface IDocumentRepository
{
    Task<DocumentModel?> GetAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<DocumentModel>> SearchAsync(string? q, CancellationToken ct = default);
    Task<DocumentModel> AddAsync(DocumentModel doc, CancellationToken ct = default);
    Task<bool> UpdateAsync(DocumentModel doc, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}