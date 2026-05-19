namespace Inedo.Extensions.Gitea.Clients;

internal sealed class CreateStatusOption(string? context, string? description, string state)
{
    public string? Context { get; } = context;
    public string? Description { get; } = description;
    public string State { get; } = state;
}
