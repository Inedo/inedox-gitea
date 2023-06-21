using System.Text.Json.Serialization;

namespace Inedo.Extensions.Gitea.Clients;

internal sealed class PullRequestBaseInfo
{
    [JsonConstructor]
    public PullRequestBaseInfo(string @ref) => this.Ref = @ref;

    public string Ref { get; }
}
