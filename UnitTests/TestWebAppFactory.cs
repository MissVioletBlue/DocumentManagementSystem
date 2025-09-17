using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

public class TestWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing"); // optional
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["UseInMemoryTestDatabase"] = "true",
                ["ConnectionStrings:DefaultConnection"] = "ignored-for-tests"
            };
            config.AddInMemoryCollection(settings);
        });
    }
}