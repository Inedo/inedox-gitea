using System.Text.Json.Serialization;

namespace Inedo.Extensions.Gitea.Clients;

[method: JsonConstructor]
internal sealed class UserInfo(string login)
{
    public string Login { get; } = login;
}
