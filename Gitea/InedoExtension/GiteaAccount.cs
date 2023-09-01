using System.ComponentModel;
using System.Security;
using Inedo.Documentation;
using Inedo.Extensibility.Git;
using Inedo.Extensibility.IssueTrackers;
using Inedo.Serialization;
using Inedo.Web;

namespace Inedo.Extensions.Gitea;

[DisplayName("Gitea Account")]
[Description("Use an account on Gitea to connect to Gitea resources.")]
public sealed class GiteaAccount : GitServiceCredentials<GiteaServiceInfo>, IIssueTrackerServiceCredentials
{
    [Required]
    [Persistent]
    [DisplayName("User name")]
    public override string? UserName { get; set; }

    [Required]
    [Persistent(Encrypted = true)]
    [DisplayName("Personal access token")]
    [FieldEditMode(FieldEditMode.Password)]
    public override SecureString? Password { get; set; }

    public override RichDescription GetCredentialDescription() => new(this.UserName);

    public override RichDescription GetServiceDescription()
    {
        return string.IsNullOrEmpty(this.ServiceUrl) || !this.TryGetServiceUrlHostName(out var hostName)
            ? new("Gitea")
            : new("Gitea (", new Hilite(hostName), ")");
    }
}
