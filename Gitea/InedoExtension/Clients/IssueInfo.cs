using System.Text.Json.Serialization;

namespace Inedo.Extensions.Gitea.Clients;

[method: JsonConstructor]
internal sealed class IssueInfo(string? body, DateTimeOffset? closedAt, string htmlUrl, long id, string title, DateTimeOffset createdAt, string state, UserInfo user, LabelInfo[]? labels)
{
    public string? Body { get; } = body;
    public DateTimeOffset? ClosedAt { get; } = closedAt;
    public string HtmlUrl { get; } = htmlUrl;
    public long Id { get; } = id;
    public string Title { get; } = title;
    public DateTimeOffset CreatedAt { get; } = createdAt;
    public string State { get; } = state;
    public UserInfo User { get; } = user;
    public LabelInfo[]? Labels { get; } = labels;
}
