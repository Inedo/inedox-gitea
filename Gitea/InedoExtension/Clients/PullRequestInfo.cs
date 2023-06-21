using System.Text.Json.Serialization;

namespace Inedo.Extensions.Gitea.Clients;

internal sealed class PullRequestInfo
{
    [JsonConstructor]
    public PullRequestInfo(long id, string url, string title, PullRequestBaseInfo @base, PullRequestBaseInfo head, string state)
    {
        this.Id = id;
        this.Url = url;
        this.Title = title;
        this.Base = @base;
        this.Head = head;
        this.State = state;
    }

    public long Id { get; }
    public string Url { get; }
    public string Title { get; }
    public PullRequestBaseInfo Base { get; }
    public PullRequestBaseInfo Head { get; }
    public string State { get; }
}
