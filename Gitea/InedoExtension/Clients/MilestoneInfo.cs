using System.Text.Json.Serialization;

namespace Inedo.Extensions.Gitea.Clients;

internal sealed class MilestoneInfo
{
    [JsonConstructor]
    public MilestoneInfo(string title) => this.Title = title;

    public string Title { get; }
}