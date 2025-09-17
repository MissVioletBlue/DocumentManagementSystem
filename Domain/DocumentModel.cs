using System.ComponentModel.DataAnnotations;

namespace Domain;

public class DocumentModel
{
    [Key]
    public int UniqueIdentifier { get; set; }
    [Required, MaxLength(256)]
    public string? DocumentTitle { get; set; }
    [MaxLength(2048)]
    public string? DocumentLocation { get; set; }
    
    // Metadata
    
    public TimeSpan? DocumentCreationDate { get; set; }
    [MaxLength(256)]
    public string? DocumentAuthor { get; set; }
    public string[]? DocumentTags { get; set; }
    
    
}