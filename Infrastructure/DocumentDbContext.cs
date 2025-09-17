using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure;

public class DocumentDbContext : DbContext
{
    public DocumentDbContext(DbContextOptions<DocumentDbContext> options) : base(options)
    {
        
    }

    public DbSet<DocumentModel> Documents => Set<DocumentModel>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<DocumentModel>().Property(document => document.DocumentTags).HasColumnType("text[]");
    }
}