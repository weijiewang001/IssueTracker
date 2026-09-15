
using IssueTracker.Service.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("IssueTrackerDb")
        ?? throw new InvalidOperationException("Connection string 'IssueTrackerDb' is missing");


builder.Services.AddDbContext<IssueTrackerDbContext>(options => options.UseSqlServer(connectionString));

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .WithTags("Diagnostics");


using (var scope  = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<IssueTrackerDbContext>();

    await context.Database.MigrateAsync();
}
app.Run();