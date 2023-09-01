using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Inedo.Diagnostics;
using Inedo.Extensibility.Git;
using Inedo.Extensibility.IssueTrackers;

namespace Inedo.Extensions.Gitea.Clients;

internal sealed class GiteaClient : ILogSink
{
    private readonly HttpClient httpClient;
    private readonly ILogSink? log;
    void ILogSink.Log(IMessage message) => this.log?.Log(message);

    public GiteaClient(string baseUrl, string? token, ILogSink? log = null)
    {
        if (string.IsNullOrEmpty(baseUrl))
            throw new ArgumentNullException(nameof(baseUrl));

        Uri uri;

        if (baseUrl.EndsWith("/api/v1"))
            uri = new Uri(baseUrl + "/");
        else if (baseUrl.EndsWith("/api/v1/"))
            uri = new Uri(baseUrl);
        else
            uri = new Uri(baseUrl.TrimEnd('/') + "/api/v1/");

        this.log = log;

        this.httpClient = SDK.CreateHttpClient();
        this.httpClient.BaseAddress = uri;
        this.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (!string.IsNullOrWhiteSpace(token))
            this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", token);
        else
            this.LogDebug("No Access Token specified for Gitea connection.");
    }

    private Task<Stream> GetHttpStreamAsync(string requestUri, CancellationToken cancellationToken)
    {
        this.LogDebug($"Getting {this.httpClient.BaseAddress}{requestUri}...");
        return this.httpClient.GetStreamAsync(requestUri, cancellationToken);
    }

    public async IAsyncEnumerable<string> GetOrganizationsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var stream = await this.GetHttpStreamAsync("orgs", cancellationToken).ConfigureAwait(false);
        var orgs = await JsonSerializer.DeserializeAsync(stream, GiteaJsonContext.Default.OrganizationOrRepoInfoArray, cancellationToken);
        if (orgs != null)
        {
            foreach (var o in orgs)
                yield return o.Name;
        }
    }
    public async IAsyncEnumerable<string> GetRepositoriesAsync(string owner, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var stream = await this.GetHttpStreamAsync($"orgs/{Uri.EscapeDataString(owner)}/repos", cancellationToken).ConfigureAwait(false);
        var orgs = await JsonSerializer.DeserializeAsync(stream, GiteaJsonContext.Default.OrganizationOrRepoInfoArray, cancellationToken);
        if (orgs != null)
        {
            foreach (var o in orgs)
                yield return o.Name;
        }
    }

    public async IAsyncEnumerable<GitRemoteBranch> GetBranchesAsync(string owner, string repo, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var url = $"repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repo)}/branches";
        using var stream = await this.GetHttpStreamAsync(url, cancellationToken).ConfigureAwait(false);
        var branches = await JsonSerializer.DeserializeAsync(stream, GiteaJsonContext.Default.BranchInfoArray, cancellationToken).ConfigureAwait(false);
        if (branches != null)
        {
            foreach (var b in branches)
                yield return new GitRemoteBranch(new GitObjectId(b.Commit.Id), b.Name, b.Protected);
        }
    }
    public async IAsyncEnumerable<GitPullRequest> GetPullRequestsAsync(string owner, string repo, bool includeClosed, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var url = $"repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repo)}/pulls?state={(includeClosed ? "all" : "open")}";
        using var stream = await this.GetHttpStreamAsync(url, cancellationToken).ConfigureAwait(false);
        var pulls = await JsonSerializer.DeserializeAsync(stream, GiteaJsonContext.Default.PullRequestInfoArray, cancellationToken).ConfigureAwait(false);
        if (pulls != null)
        {
            foreach (var p in pulls)
                yield return new GitPullRequest(p.Id.ToString(), p.Url, p.Title, p.State != "open", p.Head.Ref, p.Base.Ref);
        }
    }
    public async Task<RepositoryInfo?> GetRepositoryAsync(string owner, string repo, CancellationToken cancellationToken = default)
    {
        var url = $"repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repo)}";
        using var stream = await this.GetHttpStreamAsync(url, cancellationToken).ConfigureAwait(false);
        return await JsonSerializer.DeserializeAsync(stream, GiteaJsonContext.Default.RepositoryInfo, cancellationToken).ConfigureAwait(false);
    }
    public async Task<PullRequestInfo> CreatePullRequestAsync(string owner, string repo, string @base, string head, string title, string? body, CancellationToken cancellationToken = default)
    {
        var url = $"repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repo)}/pulls";
        this.LogDebug($"Posting to {this.httpClient.BaseAddress}{url}...");
        using var response = await this.httpClient.PostAsJsonAsync(
            url,
            new CreatePullRequestInfo(@base, head, title, body),
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase },
            cancellationToken
        ).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return (await JsonSerializer.DeserializeAsync(stream, GiteaJsonContext.Default.PullRequestInfo, cancellationToken).ConfigureAwait(false))!;
    }
    public async Task MergePullRequestAsync(string owner, string repo, long id, string headCommit, string? commitMessage, string method, CancellationToken cancellationToken = default)
    {
        var url = $"repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repo)}/pulls/{id}/merge";
        this.LogDebug($"Posting to {this.httpClient.BaseAddress}{url}...");
        using var response = await this.httpClient.PostAsJsonAsync(
            url,
            new MergePullRequestInfo(method, commitMessage, null, headCommit),
            cancellationToken
        );
        response.EnsureSuccessStatusCode();
    }
    public async Task CreateCommitStatusAsync(string owner, string repo, string sha, string status, string? description, string? context, CancellationToken cancellationToken = default)
    {
        var url = $"repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repo)}/statuses/{sha}";
        this.LogDebug($"Posting to {this.httpClient.BaseAddress}{url}...");
        using var response = await this.httpClient.PostAsJsonAsync(
            url,
            new CreateStatusOption(context, description, status),
            cancellationToken
        ).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    public async IAsyncEnumerable<IssueTrackerIssue> GetIssuesAsync(GiteaProjectId project, string? milestone, string? labels, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var url = $"/repos/{Uri.EscapeDataString(project.Namespace)}/{Uri.EscapeDataString(project.RepositoryName)}/issues?type=issues&state=all";
        if (!string.IsNullOrEmpty(milestone))
            url += $"&milestone={Uri.EscapeDataString(milestone)}";
        if (!string.IsNullOrEmpty(labels))
            url += $"&labels={Uri.EscapeDataString(labels)}";

        using var stream = await this.GetHttpStreamAsync(url, cancellationToken).ConfigureAwait(false);
        var issues = await JsonSerializer.DeserializeAsync(stream, GiteaJsonContext.Default.IssueInfoArray, cancellationToken).ConfigureAwait(false);
        if (issues != null)
        {
            foreach (var i in issues)
            {
                yield return new IssueTrackerIssue(
                    Id: i.Id.ToString(),
                    Status: i.State,
                    Type: i.Labels?.FirstOrDefault()?.Name ?? "Unspecified",
                    Title: i.Title,
                    Description: i.Body ?? string.Empty,
                    Submitter: i.User.Login,
                    SubmittedDate: i.CreatedAt.UtcDateTime,
                    IsClosed: string.Equals(i.State, "closed", StringComparison.OrdinalIgnoreCase),
                    Url: i.HtmlUrl
                );
            }
        }
    }
    public async Task UpdateIssueStatusAsync(long id, GiteaProjectId project, string toStatus, CancellationToken cancellationToken)
    {
        var url = $"/repos/{Uri.EscapeDataString(project.Namespace)}/{Uri.EscapeDataString(project.RepositoryName)}/issues/{id}";

        this.LogDebug($"Patching to {this.httpClient.BaseAddress}{url}...");
        using var response = await this.httpClient.PatchAsync(
            url,
            new StringContent(new JsonObject { ["state"] = toStatus }.ToJsonString(), InedoLib.UTF8Encoding, "application/json"),
            cancellationToken
        );

        response.EnsureSuccessStatusCode();
    }
    public async IAsyncEnumerable<MilestoneInfo> GetMilestonesAsync(GiteaProjectId project, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var url = $"/repos/{Uri.EscapeDataString(project.Namespace)}/{Uri.EscapeDataString(project.RepositoryName)}/milestones";
        using var stream = await this.GetHttpStreamAsync(url, cancellationToken).ConfigureAwait(false);
        var milestones = await JsonSerializer.DeserializeAsync(stream, GiteaJsonContext.Default.MilestoneInfoArray, cancellationToken).ConfigureAwait(false);
        if (milestones != null)
        {
            foreach (var m in milestones)
                yield return m;
        }
    }
    public async Task CreateMilestoneAsync(GiteaProjectId project, IssueTrackerVersion version, CancellationToken cancellationToken)
    {
        var url = $"/repos/{Uri.EscapeDataString(project.Namespace)}/{Uri.EscapeDataString(project.RepositoryName)}/milestones";
        this.LogDebug($"Posting to {this.httpClient.BaseAddress}{url}...");
        using var response = await this.httpClient.PostAsync(url, GetMilestoneJson(version.ToString()), cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }
    public async Task UpdateMilestoneAsync(GiteaProjectId project, MilestoneInfo milestone, IssueTrackerVersion version, CancellationToken cancellationToken)
    {
        var url = $"/repos/{Uri.EscapeDataString(project.Namespace)}/{Uri.EscapeDataString(project.RepositoryName)}/milestones";
        this.LogDebug($"Posting to {this.httpClient.BaseAddress}{url}..."); 
        using var response = await this.httpClient.PatchAsync(url, GetMilestoneJson(version.Version.ToString(), version.IsClosed ? "closed" : "open", milestone.Id), cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    private static HttpContent GetMilestoneJson(string title, string? state = null, long? id = null)
    {
        var obj = new JsonObject { ["title"] = title };
        if (!string.IsNullOrEmpty(state))
            obj["state"] = state;
        if (id.HasValue)
            obj["id"] = id;

        return new StringContent(obj.ToJsonString(), InedoLib.UTF8Encoding, "application/json");
    }
}
