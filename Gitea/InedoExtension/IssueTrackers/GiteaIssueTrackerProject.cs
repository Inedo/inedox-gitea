using System.ComponentModel;
using System.Runtime.CompilerServices;
using Inedo.Documentation;
using Inedo.ExecutionEngine.Variables;
using Inedo.Extensibility.Credentials;
using Inedo.Extensibility.IssueTrackers;
using Inedo.Extensions.Gitea.Clients;
using Inedo.Serialization;

namespace Inedo.Extensions.Gitea.IssueTrackers;

[DisplayName("Gitea Issue Tracker")]
[Description("Work with issues on a Gitea Repository.")]
public sealed class GiteaIssueTrackerProject : IssueTrackerProject<GiteaAccount>
{
    private static readonly HashSet<string> validStates = new(StringComparer.OrdinalIgnoreCase) { "Open", "Closed" };

    [Persistent]
    [DisplayName("Labels")]
    [PlaceholderText("Any")]
    [Description("A list of comma separated label names. Example: bug,ui,@high, $ReleaseNumber")]
    public string? Labels { get; set; }

    private GiteaProjectId ProjectId => new(this);

    public override async Task<IssuesQueryFilter> CreateQueryFilterAsync(IVariableEvaluationContext context)
    {
        try
        {
            var milestone = (await ProcessedString.Parse(AH.CoalesceString(this.SimpleVersionMappingExpression, "$ReleaseNumber")).EvaluateValueAsync(context).ConfigureAwait(false)).AsString();
            if (string.IsNullOrEmpty(milestone))
                throw new InvalidOperationException("milestone expression is an empty string");

            var labels = string.IsNullOrEmpty(this.Labels)
                ? null
                : (await ProcessedString.Parse(AH.CoalesceString(this.SimpleVersionMappingExpression, "$ReleaseNumber")).EvaluateValueAsync(context).ConfigureAwait(false)).AsString();

            return new GiteaIssueFilter(milestone, labels);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Could not parse the simple mapping expression \"{this.SimpleVersionMappingExpression}\": {ex.Message}");
        }
    }

    public override async Task EnsureVersionAsync(IssueTrackerVersion version, ICredentialResolutionContext context, CancellationToken cancellationToken = default)
    {
        var client = this.CreateClient(context);
        await foreach (var milestone in client.GetMilestonesAsync(this.ProjectId, cancellationToken).ConfigureAwait(false))
        {
            if (string.Equals(milestone.Title, version.Version, StringComparison.OrdinalIgnoreCase))
            {
                if (milestone.Closed == version.IsClosed)
                    return;

                await client.UpdateMilestoneAsync(this.ProjectId, milestone, version, cancellationToken).ConfigureAwait(false);
                return;
            }
        }

        await client.CreateMilestoneAsync(this.ProjectId, version, cancellationToken).ConfigureAwait(false);
    }

    public override async IAsyncEnumerable<IssueTrackerIssue> EnumerateIssuesAsync(IIssuesEnumerationContext context, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var filter = (GiteaIssueFilter)context.Filter;
        await foreach (var issue in this.CreateClient(context).GetIssuesAsync(this.ProjectId, filter.Milestone, filter.Labels, cancellationToken).ConfigureAwait(false))
            yield return new IssueTrackerIssue(issue.Id, issue.Status, issue.Type, issue.Title, issue.Description, issue.Submitter, issue.SubmittedDate, issue.IsClosed, issue.Url);
    }

    public override async IAsyncEnumerable<IssueTrackerVersion> EnumerateVersionsAsync(ICredentialResolutionContext context, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var milestone in this.CreateClient(context).GetMilestonesAsync(this.ProjectId, cancellationToken).ConfigureAwait(false))
            yield return new IssueTrackerVersion(milestone.Title, milestone.Closed);
    }

    public override RichDescription GetDescription() => new($"{this.Namespace}/{this.ProjectName}");

    public override async Task TransitionIssuesAsync(string? fromStatus, string toStatus, string? comment, IIssuesEnumerationContext context, CancellationToken cancellationToken = default)
    {
        if (!validStates.Contains(toStatus))
            throw new ArgumentOutOfRangeException($"Gitea Issue status cannot be set to \"{toStatus}\", only Open or Closed.");
        if (!string.IsNullOrEmpty(fromStatus) && !validStates.Contains(fromStatus))
            throw new ArgumentOutOfRangeException($"Gitea Issue status cannot be to \"{toStatus}\", only Open or Closed.");

        var client = this.CreateClient(context);
        var filter = (GiteaIssueFilter)context.Filter;
        await foreach (var issue in this.CreateClient(context).GetIssuesAsync(this.ProjectId, filter.Milestone, filter.Labels, cancellationToken).ConfigureAwait(false))
        {
            if (string.Equals(toStatus, issue.Status, StringComparison.OrdinalIgnoreCase))
                continue;
            if (fromStatus != null && !string.Equals(fromStatus, issue.Status, StringComparison.OrdinalIgnoreCase))
                continue;

            await client.UpdateIssueStatusAsync(long.Parse(issue.Id), this.ProjectId, toStatus, cancellationToken).ConfigureAwait(false);
        }
    }
    private GiteaClient CreateClient(ICredentialResolutionContext context)
    {
        var creds = this.GetCredentials(context) as GiteaAccount
            ?? throw new InvalidOperationException("Credentials are required to query Gitea API.");
        if (creds.ServiceUrl == null)
            throw new InvalidOperationException("serviceUrl is required to query Gitea API.");

        return new GiteaClient(creds.ServiceUrl!, AH.Unprotect(creds.Password), this);
    }
}