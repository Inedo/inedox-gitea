using System.Text.Json.Serialization;
using Inedo.Extensibility.Git;

namespace Inedo.Extensions.Gitea.Clients;

internal sealed class RepositoryInfo : IGitRepositoryInfo
{
    [JsonConstructor]
    public RepositoryInfo(string cloneUrl, string htmlUrl, string defaultBranch)
    {
        this.CloneUrl = cloneUrl;
        this.HtmlUrl = htmlUrl;
        this.DefaultBranch = defaultBranch;
    }

    [JsonPropertyName("clone_url")]
    public string CloneUrl { get; }
    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; }
    [JsonPropertyName("default_branch")]
    public string DefaultBranch { get; }

    string IGitRepositoryInfo.RepositoryUrl => this.CloneUrl;
    string? IGitRepositoryInfo.BrowseUrl => this.HtmlUrl;

    string? IGitRepositoryInfo.GetBrowseUrlForTarget(GitBrowseTarget target)
    {
        var url = this.HtmlUrl.AsSpan().TrimEnd('/');

        return target.Type switch
        {
            GitBrowseTargetType.Commit => $"{url}/commit/{target.Value}",
            GitBrowseTargetType.Tag => $"{url}/src/tag/{Uri.EscapeDataString(target.Value)}",
            GitBrowseTargetType.Branch => $"{url}/src/branch/{Uri.EscapeDataString(target.Value)}",
            _ => throw new ArgumentOutOfRangeException(nameof(target))
        };
    }
}
