using System.Text.Json.Serialization;

namespace Inedo.Extensions.Gitea.Clients;

internal sealed class OrganizationOrRepoInfo
{
    [JsonConstructor]
    public OrganizationOrRepoInfo(string name) => this.Name = name;

    public string Name { get; }
}
