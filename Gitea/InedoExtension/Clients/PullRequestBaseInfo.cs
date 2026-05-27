using System.Text.Json.Serialization;

namespace Inedo.Extensions.Gitea.Clients;

[method: JsonConstructor]
internal sealed class PullRequestBaseInfo(string @ref)
{
    public string Ref { get; } = @ref;
}
