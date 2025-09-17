using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure;

public class DocumentDbContext : DbContext
{
    public DbSet<DocumentModel> Documents { get; set; } = null!;

    public DocumentDbContext(DbContextOptions<DocumentDbContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<DocumentModel>().Property(document => document.DocumentTags).HasColumnType("text[]");
    }
}