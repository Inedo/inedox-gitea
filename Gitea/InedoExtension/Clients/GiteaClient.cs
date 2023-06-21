using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Inedo.Extensibility.Git;

namespace Inedo.Extensions.Gitea.Clients;

internal sealed class GiteaClient
{
    private readonly HttpClient httpClient;

    public GiteaClient(string baseUrl, string? token)
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

        this.httpClient = new HttpClient { BaseAddress = uri };
        this.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (!string.IsNullOrWhiteSpace(token))
            this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", token);
    }

    public async IAsyncEnumerable<string> GetOrganizationsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var stream = await this.httpClient.GetStreamAsync("orgs", cancellationToken).ConfigureAwait(false);
        var orgs = await JsonSerializer.DeserializeAsync(stream, GiteaJsonContext.Default.OrganizationOrRepoInfoArray, cancellationToken);
        if (orgs != null)
        {
            foreach (var o in orgs)
                yield return o.Name;
        }
    }
    public async IAsyncEnumerable<string> GetRepositoriesAsync(string owner, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var stream = await this.httpClient.GetStreamAsync($"orgs/{Uri.EscapeDataString(owner)}/repos", cancellationToken).ConfigureAwait(false);
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
        using var stream = await this.httpClient.GetStreamAsync(url, cancellationToken).ConfigureAwait(false);
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
        using var stream = await this.httpClient.GetStreamAsync(url, cancellationToken).ConfigureAwait(false);
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
        using var stream = await this.httpClient.GetStreamAsync(url, cancellationToken).ConfigureAwait(false);
        return await JsonSerializer.DeserializeAsync(stream, GiteaJsonContext.Default.RepositoryInfo, cancellationToken).ConfigureAwait(false);
    }
    public async Task<PullRequestInfo> CreatePullRequestAsync(string owner, string repo, string @base, string head, string title, string? body, CancellationToken cancellationToken = default)
    {
        var url = $"repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repo)}/pulls";
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
        using var response = await this.httpClient.PostAsJsonAsync(
            url,
            new CreateStatusOption(context, description, status),
            cancellationToken
        ).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    public async IAsyncEnumerable<IssueInfo> GetIssuesAsync(string owner, string repo, string? milestone, string? labels, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var url = $"/repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repo)}/issues?type=issues&state=all";
        if (!string.IsNullOrEmpty(milestone))
            url += $"&milestone={Uri.EscapeDataString(milestone)}";
        if (!string.IsNullOrEmpty(labels))
            url += $"&labels={Uri.EscapeDataString(labels)}";

        using var stream = await this.httpClient.GetStreamAsync(url, cancellationToken).ConfigureAwait(false);
        var issues = await JsonSerializer.DeserializeAsync(stream, GiteaJsonContext.Default.IssueInfoArray, cancellationToken).ConfigureAwait(false);
        if (issues != null)
        {
            foreach (var i in issues)
                yield return i;
        }
    }
    public async IAsyncEnumerable<string> GetMilestonesAsync(string owner, string repo, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var url = $"/repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repo)}/milestones";
        using var stream = await this.httpClient.GetStreamAsync(url, cancellationToken).ConfigureAwait(false);
        var milestones = await JsonSerializer.DeserializeAsync(stream, GiteaJsonContext.Default.MilestoneInfoArray, cancellationToken).ConfigureAwait(false);
        if (milestones != null)
        {
            foreach (var m in milestones)
                yield return m.Title;
        }
    }
}
