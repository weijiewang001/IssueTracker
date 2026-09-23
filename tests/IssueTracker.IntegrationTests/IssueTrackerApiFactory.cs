using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.MsSql;

namespace IssueTracker.IntegrationTests;

public class IssueTrackerApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer container
        = new MsSqlBuilder("mcr.microsoft.com/mssql/server").Build();

    public async Task InitializeAsync() => await container.StartAsync();

    public new async Task DisposeAsync()
    {
        await container.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:IssueTrackerDb",
            container.GetConnectionString());

        builder.UseEnvironment("Testing");
    }


}
