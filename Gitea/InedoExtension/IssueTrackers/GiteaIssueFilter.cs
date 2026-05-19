using Inedo.Extensibility.IssueTrackers;

namespace Inedo.Extensions.Gitea.IssueTrackers;

internal class GiteaIssueFilter(string milestone, string? labels) : IssuesQueryFilter
{
    public string Milestone { get; } = milestone;
    public string? Labels { get; } = labels;
}
