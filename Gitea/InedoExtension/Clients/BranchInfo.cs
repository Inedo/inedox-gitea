using System.Text.Json.Serialization;

namespace Inedo.Extensions.Gitea.Clients;

internal sealed class BranchInfo
{
    [JsonConstructor]
    public BranchInfo(string name, CommitInfo commit, bool @protected)
    {
        this.Name = name;
        this.Commit = commit;
        this.Protected = @protected;
    }

    public string Name { get; }
    public CommitInfo Commit { get; }
    public bool Protected { get; }
}
