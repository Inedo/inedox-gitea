namespace Inedo.Extensions.Gitea.SuggestionProviders;

internal sealed class GiteaOrganizationSuggestionProvider : GiteaSuggestionProvider
{
    protected override IAsyncEnumerable<string>? GetSuggestionsAsync(CancellationToken cancellationToken) => this.Client?.GetOrganizationsAsync(cancellationToken);
}
