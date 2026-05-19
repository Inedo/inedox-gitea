namespace Inedo.Extensions.Gitea.Clients;

internal sealed class CreatePullRequestInfo(string @base, string head, string title, string? body)
{
    public string Base { get; } = @base;
    public string Head { get; } = head;
    public string Title { get; } = title;
    public string? Body { get; } = body;
}
