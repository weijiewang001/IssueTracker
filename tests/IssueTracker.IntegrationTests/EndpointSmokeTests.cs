using IssueTracker.Contracts;
using IssueTracker.Domain.Issues;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using static System.Net.WebRequestMethods;

namespace IssueTracker.IntegrationTests;

public class EndpointSmokeTests(IssueTrackerApiFactory factory)
    : IClassFixture<IssueTrackerApiFactory>
{
    private readonly HttpClient client = factory.CreateClient();

    [Fact]
    public async Task Create_Returns_201_And_Body()
    {
        var response = await client.PostAsJsonAsync("/api/issues", new CreateIssueRequest(
            Title: "First issue",
            Description: "Smoke testing the API end to end",
            Priority: Priority.Medium,
            ReportedBy: "filip"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<IssueResponse>();
        Assert.NotNull(created);
        Assert.True(created!.Id > 0);
        Assert.Equal("First issue", created.Title);
    }

    [Fact]
    public async Task Start_Moves_Issue_To_InProgress()
    {
        var created = await CreateAnIssueAsync();

        var response = await client.PostAsJsonAsync(
            $"/api/issues/{created.Id}/start", new StartIssueRequest("anna"));
        var started = await response.Content.ReadFromJsonAsync<IssueResponse>();

        Assert.Equal(IssueStatus.InProgress, started!.Status);
        Assert.Equal("anna", started.AssignedTo);
    }

    [Fact]
    public async Task List_Returns_200()
    {
        var response = await client.GetAsync("/api/issues");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Returns_The_Issue()
    {
        var created = await CreateAnIssueAsync();

        var fetched = await client.GetFromJsonAsync<IssueResponse>($"/api/issues/{created.Id}");

        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal(created.Title, fetched.Title);
    }

    [Fact]
    public async Task Update_Changes_The_Title()
    {
        var created = await CreateAnIssueAsync();

        var response = await client.PutAsJsonAsync($"/api/issues/{created.Id}",
            new UpdateIssueRequest("Renamed", "Different description", Priority.High));
        var updated = await response.Content.ReadFromJsonAsync<IssueResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Renamed", updated!.Title);
        Assert.Equal(Priority.High, updated.Priority);
    }

    [Fact]
    public async Task Close_Moves_Issue_To_Closed()
    {
        var created = await CreateAnIssueAsync();
        await client.PostAsJsonAsync(
            $"/api/issues/{created.Id}/start", new StartIssueRequest("anna"));

        var response = await client.PostAsync($"/api/issues/{created.Id}/close", content: null);
        var closed = await response.Content.ReadFromJsonAsync<IssueResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(IssueStatus.Closed, closed!.Status);
    }

    [Fact]
    public async Task Delete_Removes_The_Issue()
    {
        var created = await CreateAnIssueAsync();

        var deleteResponse = await client.DeleteAsync($"/api/issues/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var afterDelete = await client.GetAsync($"/api/issues/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, afterDelete.StatusCode);
    }

    private async Task<IssueResponse> CreateAnIssueAsync()
    {
        var response = await client.PostAsJsonAsync("/api/issues", new CreateIssueRequest(
            Title: "Test issue", Description: null,
            Priority: Priority.Low, ReportedBy: "filip"));
        return (await response.Content.ReadFromJsonAsync<IssueResponse>())!;
    }
}
