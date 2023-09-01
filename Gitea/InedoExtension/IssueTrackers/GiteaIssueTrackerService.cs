using System.ComponentModel;
using Inedo.Extensibility.Git;
using Inedo.Extensibility.IssueTrackers;
using Inedo.Extensions.Gitea.Clients;

namespace Inedo.Extensions.Gitea.IssueTrackers;

[DisplayName("Gitea")]
[Description("Provides integration for issues on Gitea repositories.")]
public class GiteaIssueTrackerService : IssueTrackerService<GiteaIssueTrackerProject, GiteaAccount>
{
    public override string ServiceName => "Gitea";
    public override string DefaultVersionFieldName => "Milestone";
    public override bool HasDefaultApiUrl => false;
    public override string PasswordDisplayName => "Personal access token";
    public override string ApiUrlPlaceholderText => "e.g. https://git.mycorp.local/api/v1/";


    protected override IAsyncEnumerable<string> GetNamespacesAsync(GiteaAccount credentials, CancellationToken cancellationToken = default)
    {
        return GetClient(credentials).GetOrganizationsAsync(cancellationToken);
    }
    protected override IAsyncEnumerable<string> GetProjectNamesAsync(GiteaAccount credentials, string? serviceNamespace = null, CancellationToken cancellationToken = default)
    {
        return GetClient(credentials).GetRepositoriesAsync(serviceNamespace!, cancellationToken);
    }

    private static GiteaClient GetClient(GitServiceCredentials credentials)
    {
        if (string.IsNullOrWhiteSpace(credentials.ServiceUrl))
            throw new InvalidOperationException("Gitea requires a service url.");

        return new GiteaClient(credentials.ServiceUrl, AH.Unprotect(credentials.Password));
    }

}