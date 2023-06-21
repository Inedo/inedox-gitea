namespace Inedo.Extensions.Gitea.Clients;

internal sealed class CreatePullRequestInfo
{
    public CreatePullRequestInfo(string @base, string head, string title, string? body)
    {
        this.Base = @base;
        this.Head = head;
        this.Title = title;
        this.Body = body;
    }

    public string Base { get; }
    public string Head { get; }
    public string Title { get; }
    public string? Body { get; }
}
