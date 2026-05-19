using System.Text.Json.Serialization;

namespace Inedo.Extensions.Gitea.Clients;

[method: JsonConstructor]
internal sealed class CommitInfo(string id)
{
    public string Id { get; } = id;
}
