using System.Text.Json.Serialization;

namespace Inedo.Extensions.Gitea.Clients;

internal sealed class LabelInfo
{
    [JsonConstructor]
    public LabelInfo(string name) => this.Name = name;

    public string Name { get; }
}
