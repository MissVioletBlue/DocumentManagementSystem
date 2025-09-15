using System.ComponentModel.DataAnnotations;

namespace Domain;

public class DocumentModel
{
    [Required]
    public int UniqueIdentifier { get; set; }
    public string? DocumentTitle { get; set; }
    public string? DocumentLocation { get; set; }
    
    // Metadata
    
    public TimeSpan? DocumentCreationDate { get; set; }
    public string? DocumentAuthor { get; set; }
    public List<string>? DocumentTags { get; set; }
    
    
}