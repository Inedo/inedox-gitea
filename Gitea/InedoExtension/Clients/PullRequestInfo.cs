using System.Text.Json.Serialization;

namespace Inedo.Extensions.Gitea.Clients;

[method: JsonConstructor]
internal sealed class PullRequestInfo(long id, string url, string title, PullRequestBaseInfo @base, PullRequestBaseInfo head, string state)
{
    public long Id { get; } = id;
    public string Url { get; } = url;
    public string Title { get; } = title;
    public PullRequestBaseInfo Base { get; } = @base;
    public PullRequestBaseInfo Head { get; } = head;
    public string State { get; } = state;
}
