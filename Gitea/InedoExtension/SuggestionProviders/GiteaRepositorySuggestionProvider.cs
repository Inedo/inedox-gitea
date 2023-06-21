namespace Inedo.Extensions.Gitea.SuggestionProviders;

internal sealed class GiteaRepositorySuggestionProvider : GiteaSuggestionProvider
{
    protected override IAsyncEnumerable<string>? GetSuggestionsAsync(CancellationToken cancellationToken)
    {
        if (this.Client != null && !string.IsNullOrEmpty(this.Resource?.OrganizationName))
            return this.Client.GetRepositoriesAsync(this.Resource.OrganizationName, cancellationToken);
        else
            return null;
    }
}
