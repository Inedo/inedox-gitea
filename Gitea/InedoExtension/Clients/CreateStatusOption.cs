namespace Inedo.Extensions.Gitea.Clients;

internal sealed class CreateStatusOption
{
    public CreateStatusOption(string? context, string? description, string state)
    {
        this.Context = context;
        this.Description = description;
        this.State = state;
    }

    public string? Context { get; }
    public string? Description { get; }
    public string State { get; }
}
