namespace Application.Documents;

public record CreateDocumentDto(
    string DocumentTitle,
    string? DocumentLocation,
    string? DocumentAuthor,
    string[]? DocumentTags
);

public record UpdateDocumentDto(
    string DocumentTitle,
    string? DocumentLocation,
    string? DocumentAuthor,
    string[]? DocumentTags
);

public record DocumentDto(
    int UniqueIdentifier,
    string DocumentTitle,
    string? DocumentLocation,
    string? DocumentAuthor,
    string[]? DocumentTags
);