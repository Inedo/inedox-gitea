using System.Text.Json.Serialization;
using Inedo.Extensibility.IssueSources;

namespace Inedo.Extensions.Gitea.Clients;

internal sealed class IssueInfo
{
    [JsonConstructor]
    public IssueInfo(string? body, DateTimeOffset? closedAt, string htmlUrl, long id, string title, DateTimeOffset createdAt, string state, UserInfo user, LabelInfo[]? labels)
    {
        this.Body = body;
        this.ClosedAt = closedAt;
        this.HtmlUrl = htmlUrl;
        this.Id = id;
        this.Title = title;
        this.CreatedAt = createdAt;
        this.State = state;
        this.User = user;
        this.Labels = labels;
    }

    public string? Body { get; }
    [JsonPropertyName("closed_at")]
    public DateTimeOffset? ClosedAt { get; }
    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; }
    public long Id { get; }
    public string Title { get; }
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; }
    public string State { get; }
    public UserInfo User { get; }
    public LabelInfo[]? Labels { get; }
}
