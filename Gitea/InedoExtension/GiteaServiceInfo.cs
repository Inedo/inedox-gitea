using System.ComponentModel;
using Inedo.Extensibility.Git;
using Inedo.Extensions.Gitea.Clients;

namespace Inedo.Extensions.Gitea;

[DisplayName("Gitea")]
[Description("Provides integration for Gitea repositories.")]
public sealed class GiteaServiceInfo : GitService<GiteaRepository, GiteaAccount>
{
    public override string ServiceName => "Gitea";
    public override bool HasDefaultApiUrl => true;
    public override string PasswordDisplayName => "Personal access token";

    public override IAsyncEnumerable<string> GetNamespacesAsync(GitServiceCredentials credentials, CancellationToken cancellationToken = default)
    {
        return GetClient(credentials).GetOrganizationsAsync(cancellationToken);
    }
    public override IAsyncEnumerable<string> GetRepositoryNamesAsync(GitServiceCredentials credentials, string serviceNamespace, CancellationToken cancellationToken = default)
    {
        return GetClient(credentials).GetRepositoriesAsync(serviceNamespace, cancellationToken);
    }

    private static GiteaClient GetClient(GitServiceCredentials credentials)
    {
        if (string.IsNullOrWhiteSpace(credentials.ServiceUrl))
            throw new InvalidOperationException("Gitea requires a service url.");

        return new GiteaClient(credentials.ServiceUrl, AH.Unprotect(credentials.Password));
    }
}
