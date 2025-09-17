using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure;

public class DocumentDbContext : DbContext
{
    private readonly IConfiguration _configuration;
    
    public DbSet<DocumentModel> Documents { get; set; }
    
    public DocumentDbContext(IConfiguration configuration){
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_configuration.GetConnectionString("DefaultConnection"));
    }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<DocumentModel>().Property(document => document.DocumentTags).HasColumnType("text[]");
    }
}