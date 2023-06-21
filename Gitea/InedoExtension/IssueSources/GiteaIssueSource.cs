using System.ComponentModel;
using System.Runtime.CompilerServices;
using Inedo.Documentation;
using Inedo.Extensibility.Credentials;
using Inedo.Extensibility.IssueSources;
using Inedo.Extensibility.SecureResources;
using Inedo.Extensions.Gitea.Clients;
using Inedo.Extensions.Gitea.SuggestionProviders;
using Inedo.Serialization;
using Inedo.Web;

namespace Inedo.Extensions.Gitea.IssueSources;

[DisplayName("Gitea Issue Source")]
[Description("Issue source for Gitea based on milestones.")]
public sealed class GiteaIssueSource : IssueSource<GiteaRepository>
{
    [Persistent]
    [DisplayName("Repository name")]
    [PlaceholderText("Use repository name from credentials")]
    [SuggestableValue(typeof(GiteaRepositorySuggestionProvider))]
    public string? RepositoryName { get; set; }

    [Persistent]
    [DisplayName("Milestone")]
    [SuggestableValue(typeof(GiteaMilestoneSuggestionProvider))]
    public string? MilestoneTitle { get; set; }

    [Persistent]
    [DisplayName("Labels")]
    [PlaceholderText("any")]
    [Description("A list of comma separated label names. Example: bug,ui,@high")]
    public string? Labels { get; set; }

    public override async IAsyncEnumerable<IIssueTrackerIssue> EnumerateIssuesAsync(IIssueSourceEnumerationContext context, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var resource = SecureResource.TryCreate(this.ResourceName, new ResourceResolutionContext(context.ProjectId)) as GiteaRepository;
        var credentials = resource?.GetCredentials(new CredentialResolutionContext(context.ProjectId, null)) as GiteaAccount;
        if (resource == null)
            throw new InvalidOperationException("A resource must be supplied to enumerate Gitea issues.");

        var repositoryName = AH.CoalesceString(this.RepositoryName, resource.RepositoryName);
        if (string.IsNullOrEmpty(repositoryName))
            throw new InvalidOperationException("A repository name must be defined in either the issue source or associated Gitea credentials in order to enumerate Gitea issues.");

        if (string.IsNullOrWhiteSpace(credentials?.ServiceUrl))
            throw new InvalidOperationException("Gitea requires a service url.");

        var client = new GiteaClient(credentials.ServiceUrl!, AH.Unprotect(credentials.Password));
        await foreach (var i in client.GetIssuesAsync(resource.OrganizationName!, resource.RepositoryName!, this.MilestoneTitle, this.Labels, cancellationToken).ConfigureAwait(false))
            yield return i;
    }

    public override RichDescription GetDescription() => new("Get Issues from Gitea");
}
