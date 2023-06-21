namespace Inedo.Extensions.Gitea.SuggestionProviders;

internal sealed class GiteaMilestoneSuggestionProvider : GiteaSuggestionProvider
{
    protected override IAsyncEnumerable<string>? GetSuggestionsAsync(CancellationToken cancellationToken)
    {
        if (this.Client != null && !string.IsNullOrEmpty(this.Resource?.OrganizationName) && !string.IsNullOrEmpty(this.Resource.RepositoryName))
            return this.Client.GetMilestonesAsync(this.Resource.OrganizationName, this.Resource.RepositoryName, cancellationToken);
        else
            return null;
    }
}
