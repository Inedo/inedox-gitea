using System.Text.Json.Serialization;

namespace Inedo.Extensions.Gitea.Clients;

internal sealed class MilestoneInfo
{
    [JsonConstructor]
    public MilestoneInfo(long id, string title, string state)
    {
        this.Id = id;
        this.Title = title;
        this.State = state;
    }

    public long Id { get; }
    public string Title { get; }
    public string State { get; }

    [JsonIgnore]
    public bool Closed => string.Equals(this.State, "closed", StringComparison.OrdinalIgnoreCase);
}