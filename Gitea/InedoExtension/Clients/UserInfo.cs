using System.Text.Json.Serialization;

namespace Inedo.Extensions.Gitea.Clients;

internal sealed class UserInfo
{
    [JsonConstructor]
    public UserInfo(string login) => this.Login = login;

    public string Login { get; }
}
