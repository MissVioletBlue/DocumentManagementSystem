using System.Net;
using System.Net.Http.Json;
using Application.Documents;
using FluentAssertions;

namespace UnitTests;

public class ApiCrudTests
{
    private TestWebAppFactory _factory = default!;
    private HttpClient _client = default!;

    [SetUp]
    public void Setup()
    {
        _factory = new TestWebAppFactory();
        _client  = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [Test]
    public async Task Health_Returns_Ok()
    {
        var res = await _client.GetAsync("/health");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task Create_Get_Update_Delete_Work_EndToEnd()
    {
        // Create
        var create = new CreateDocumentDto(
            DocumentTitle: "API Doc",
            DocumentLocation: null,
            DocumentAuthor: "Matteo",
            DocumentTags: new[] { "swen3" }
        );

        var post = await _client.PostAsJsonAsync("/api/documents", create);
        post.StatusCode.Should().Be(HttpStatusCode.Created);
        post.Headers.Location.Should().NotBeNull();

        // GET by id
        var get = await _client.GetAsync(post.Headers.Location);
        get.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await get.Content.ReadFromJsonAsync<DocumentDto>();
        dto!.DocumentTitle.Should().Be("API Doc");

        // Search
        var list = await _client.GetFromJsonAsync<DocumentDto[]>("/api/documents?q=API");
        list!.Should().ContainSingle(x => x.UniqueIdentifier == dto.UniqueIdentifier);

        // Update
        var update = new UpdateDocumentDto(
            DocumentTitle: "API Doc v2",
            DocumentLocation: "here",
            DocumentAuthor: "M",
            DocumentTags: new[] { "swen3", "updated" }
        );
        var put = await _client.PutAsJsonAsync($"/api/documents/{dto.UniqueIdentifier}", update);
        put.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update
        var get2 = await _client.GetFromJsonAsync<DocumentDto>($"/api/documents/{dto.UniqueIdentifier}");
        get2!.DocumentTitle.Should().Be("API Doc v2");
        get2.DocumentTags.Should().BeEquivalentTo(new[] { "swen3", "updated" });

        // Delete
        var del = await _client.DeleteAsync($"/api/documents/{dto.UniqueIdentifier}");
        del.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Gone
        var notFound = await _client.GetAsync($"/api/documents/{dto.UniqueIdentifier}");
        notFound.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Create_Without_Title_Returns_400()
    {
        var bad = new { documentTitle = "", documentLocation = (string?)null, documentAuthor = (string?)null, documentTags = (string[]?)null };
        var res = await _client.PostAsJsonAsync("/api/documents", bad);
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Get_Missing_Returns_404()
    {
        var res = await _client.GetAsync("/api/documents/999999");
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
