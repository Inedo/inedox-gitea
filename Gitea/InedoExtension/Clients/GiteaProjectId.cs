using Inedo.Extensions.Gitea.IssueTrackers;

namespace Inedo.Extensions.Gitea.Clients;

internal sealed record GiteaProjectId
{
    public GiteaProjectId(GiteaIssueTrackerProject project)
    {
        if (string.IsNullOrEmpty(project.Namespace))
            throw new ArgumentException("Project does not have a Namespace set.");
        if (string.IsNullOrEmpty(project.ProjectName))
            throw new ArgumentException("Project does not have a ProjectName set.");
        
        this.Namespace = project.Namespace;
        this.RepositoryName = project.ProjectName;
    }

    public string Namespace { get; }
    public string RepositoryName { get; }
}