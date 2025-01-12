﻿using System.ComponentModel;
using Inedo.Extensibility.Git;
using Inedo.Extensions.Gitea.Clients;

namespace Inedo.Extensions.Gitea;

[DisplayName("Gitea")]
[Description("Provides integration for Gitea repositories.")]
public sealed class GiteaServiceInfo : GitService<GiteaRepository, GiteaAccount>
{
    public override string ServiceName => "Gitea";
    public override bool HasDefaultApiUrl => false;
    public override string PasswordDisplayName => "Access token";
    public override string ApiUrlPlaceholderText => "e.g. https://git.mycorp.local/api/v1/";

    protected override IAsyncEnumerable<string> GetNamespacesAsync(GiteaAccount credentials, CancellationToken cancellationToken = default)
    {
        return this.GetClient(credentials).GetOrganizationsAsync(cancellationToken);
    }
    protected override IAsyncEnumerable<string> GetRepositoryNamesAsync(GiteaAccount credentials, string serviceNamespace, CancellationToken cancellationToken = default)
    {
        return this.GetClient(credentials).GetRepositoriesAsync(serviceNamespace, cancellationToken);
    }

    private GiteaClient GetClient(GitServiceCredentials credentials)
    {
        return new GiteaClient(
            AH.NullIf(credentials.ServiceUrl, "") ?? throw new InvalidOperationException("Gitea requires an API Url"),
            AH.Unprotect(credentials.Password),
            this,
            credentials.IgnoreCertificateCheck
        );
    }
}
