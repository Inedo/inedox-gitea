using System.Text.Json.Serialization;

namespace Inedo.Extensions.Gitea.Clients;

[method: JsonConstructor]
internal sealed class MilestoneInfo(long id, string title, string state)
{
    public long Id { get; } = id;
    public string Title { get; } = title;
    public string State { get; } = state;

    [JsonIgnore]
    public bool Closed => string.Equals(this.State, "closed", StringComparison.OrdinalIgnoreCase);
}
