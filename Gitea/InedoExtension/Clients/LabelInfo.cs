using System.Text.Json.Serialization;

namespace Inedo.Extensions.Gitea.Clients;

[method: JsonConstructor]
internal sealed class LabelInfo(string name)
{
    public string Name { get; } = name;
}
