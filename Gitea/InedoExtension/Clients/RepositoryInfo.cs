using System.Text.Json.Serialization;
using Inedo.Extensibility.Git;

namespace Inedo.Extensions.Gitea.Clients;

[method: JsonConstructor]
internal sealed class RepositoryInfo(string cloneUrl, string htmlUrl, string defaultBranch) : IGitRepositoryInfo
{
    public string CloneUrl { get; } = cloneUrl;
    public string HtmlUrl { get; } = htmlUrl;
    public string DefaultBranch { get; } = defaultBranch;

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
