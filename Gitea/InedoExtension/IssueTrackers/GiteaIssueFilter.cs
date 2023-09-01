using Inedo.Extensibility.IssueTrackers;

namespace Inedo.Extensions.Gitea.IssueTrackers
{
    internal class GiteaIssueFilter : IssuesQueryFilter
    {
        public GiteaIssueFilter(string milestone, string? labels)
        {
            this.Milestone = milestone;
            this.Labels = labels;
        }
        public string Milestone { get; }
        public string? Labels { get; }
    }
}