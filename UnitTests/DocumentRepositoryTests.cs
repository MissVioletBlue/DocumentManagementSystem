using System.Threading;
using System.Threading.Tasks;
using Domain;
using FluentAssertions;
using Infrastructure;
using Infrastructure.Documents;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace UnitTests;

public class DocumentRepositoryTests
{
    private DocumentRepository _repo = default!;
    private DocumentDbContext _ctx = default!;

    [SetUp]
    public void SetUp()
    {
        var opts = new DbContextOptionsBuilder<DocumentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _ctx = new DocumentDbContext(opts);
        _repo = new DocumentRepository(_ctx);
    }

    [TearDown]
    public void TearDown() => _ctx.Dispose();

    [Test]
    public async Task Add_Then_Get_Works()
    {
        var created = await _repo.AddAsync(new DocumentModel { DocumentTitle = "Spec" });
        created.UniqueIdentifier.Should().BeGreaterThan(0);

        var loaded = await _repo.GetAsync(created.UniqueIdentifier);
        loaded.Should().NotBeNull();
        loaded!.DocumentTitle.Should().Be("Spec");
    }

    [Test]
    public async Task Search_Filters_By_Title()
    {
        await _repo.AddAsync(new DocumentModel { DocumentTitle = "Alpha" });
        await _repo.AddAsync(new DocumentModel { DocumentTitle = "Beta" });

        var res = await _repo.SearchAsync("Al");
        res.Should().HaveCount(1);
        res[0].DocumentTitle.Should().Be("Alpha");
    }

    [Test]
    public async Task Update_Changes_Are_Saved()
    {
        var doc = await _repo.AddAsync(new DocumentModel { DocumentTitle = "Old" });

        doc.DocumentTitle = "New";
        var ok = await _repo.UpdateAsync(doc);
        ok.Should().BeTrue();

        var loaded = await _repo.GetAsync(doc.UniqueIdentifier);
        loaded!.DocumentTitle.Should().Be("New");
    }

    [Test]
    public async Task Delete_Removes_Row()
    {
        var doc = await _repo.AddAsync(new DocumentModel { DocumentTitle = "Temp" });
        var ok = await _repo.DeleteAsync(doc.UniqueIdentifier);
        ok.Should().BeTrue();

        var missing = await _repo.GetAsync(doc.UniqueIdentifier);
        missing.Should().BeNull();
    }
}
