using System.Text.Json.Serialization;

namespace Inedo.Extensions.Gitea.Clients;

[method: JsonConstructor]
internal sealed class OrganizationOrRepoInfo(string name)
{
    public string Name { get; } = name;
}
