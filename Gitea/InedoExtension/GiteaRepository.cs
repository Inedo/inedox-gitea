using System.ComponentModel;
using Inedo.Documentation;
using Inedo.Extensibility.Credentials;
using Inedo.Extensibility.Git;
using Inedo.Extensions.Gitea.Clients;
using Inedo.Extensions.Gitea.SuggestionProviders;
using Inedo.Serialization;
using Inedo.Web;

namespace Inedo.Extensions.Gitea;

[DisplayName("Gitea Project")]
[Description("Connect to a Gitea project for source code or issue tracking integration.")]
public sealed class GiteaRepository : GitServiceRepository<GiteaAccount>
{
    [Persistent]
    [DisplayName("Organization name")]
    [SuggestableValue(typeof(GiteaOrganizationSuggestionProvider))]
    public string? OrganizationName { get; set; }

    [Persistent]
    [DisplayName("Repository")]
    [SuggestableValue(typeof(GiteaRepositorySuggestionProvider))]
    public override string? RepositoryName { get; set; }

    public override string? Namespace
    {
        get => this.OrganizationName;
        set => this.OrganizationName = value;
    }

    public override RichDescription GetDescription()
    {
        var group = string.IsNullOrEmpty(this.OrganizationName) ? "" : $"{this.OrganizationName}/";
        return new RichDescription($"{group}{this.RepositoryName}");
    }

    public override IAsyncEnumerable<GitPullRequest> GetPullRequestsAsync(ICredentialResolutionContext context, bool includeClosed = false, CancellationToken cancellationToken = default)
    {
        return this.GetClient(context).GetPullRequestsAsync(this.OrganizationName!, this.RepositoryName!, includeClosed, cancellationToken);
    }

    public override async Task<IGitRepositoryInfo> GetRepositoryInfoAsync(ICredentialResolutionContext context, CancellationToken cancellationToken = default)
    {
        return await this.GetClient(context).GetRepositoryAsync(this.OrganizationName!, this.RepositoryName!, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Repository not found.");
    }

    public override async Task<string> CreatePullRequestAsync(ICredentialResolutionContext context, string sourceBranch, string targetBranch, string title, string? description = null, CancellationToken cancellationToken = default)
    {
        var pr = await this.GetClient(context).CreatePullRequestAsync(this.OrganizationName!, this.RepositoryName!, targetBranch, sourceBranch, title, description, cancellationToken).ConfigureAwait(false);
        return pr.Id.ToString();
    }

    public override Task MergePullRequestAsync(ICredentialResolutionContext context, string id, string headCommit, string? commitMessage = null, string? method = null, CancellationToken cancellationToken = default)
    {
        return this.GetClient(context).MergePullRequestAsync(
            this.OrganizationName!,
            this.RepositoryName!,
            long.Parse(id),
            headCommit,
            commitMessage,
            AH.CoalesceString(method, "merge"),
            cancellationToken
        );
    }

    public override Task SetCommitStatusAsync(ICredentialResolutionContext context, string commit, string status, string? description = null, string? statusContext = null, CancellationToken cancellationToken = default)
    {
        return this.GetClient(context).CreateCommitStatusAsync(
            this.OrganizationName!,
            this.RepositoryName!,
            commit,
            status,
            description,
            statusContext,
            cancellationToken
        );
    }

    public override IAsyncEnumerable<GitRemoteBranch> GetRemoteBranchesAsync(ICredentialResolutionContext context, CancellationToken cancellationToken = default)
    {
        return this.GetClient(context).GetBranchesAsync(this.OrganizationName!, this.RepositoryName!, cancellationToken);
    }

    private GiteaClient GetClient(ICredentialResolutionContext context)
    {
        var account = (GiteaAccount?)this.GetCredentials(context) ?? throw new InvalidOperationException("Credentials not found for Gitea repository.");
        if (string.IsNullOrWhiteSpace(account.ServiceUrl))
            throw new InvalidOperationException("Gitea requires a service url.");

        return new GiteaClient(account.ServiceUrl, AH.Unprotect(account.Password));
    }
}
