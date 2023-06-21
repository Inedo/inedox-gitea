using System.Text.Json.Serialization;

namespace Inedo.Extensions.Gitea.Clients;

internal sealed class CommitInfo
{
    [JsonConstructor]
    public CommitInfo(string id) => this.Id = id;

    public string Id { get; }
}
