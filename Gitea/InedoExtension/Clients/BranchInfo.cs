using System.Text.Json.Serialization;

namespace Inedo.Extensions.Gitea.Clients;

[method: JsonConstructor]
internal sealed class BranchInfo(string name, CommitInfo commit, bool @protected)
{
    public string Name { get; } = name;
    public CommitInfo Commit { get; } = commit;
    public bool Protected { get; } = @protected;
}
