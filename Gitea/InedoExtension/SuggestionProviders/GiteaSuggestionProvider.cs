using System.Runtime.CompilerServices;
using Inedo.Extensibility;
using Inedo.Extensibility.Credentials;
using Inedo.Extensibility.SecureResources;
using Inedo.Extensions.Gitea.Clients;
using Inedo.Web;

namespace Inedo.Extensions.Gitea.SuggestionProviders;

internal abstract class GiteaSuggestionProvider : ISuggestionProvider
{
    protected GiteaSuggestionProvider()
    {
    }

    public GiteaAccount? Credentials { get; private set; }
    public GiteaRepository? Resource { get; private set; }
    public IComponentConfiguration? ComponentConfiguration { get; private set; }
    public GiteaClient? Client { get; private set; }

    public async Task<IEnumerable<string>> GetSuggestionsAsync(IComponentConfiguration config)
    {
        var list = new List<string>();
        await foreach (var s in this.GetSuggestionsAsync(string.Empty, config, default).ConfigureAwait(false))
            list.Add(s);
        return list;
    }

    public async IAsyncEnumerable<string> GetSuggestionsAsync(string startsWith, IComponentConfiguration config, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var context = new CredentialResolutionContext((config.EditorContext as ICredentialResolutionContext)?.ApplicationId, null);

        // resource editors
        var credentialName = config[nameof(GiteaRepository.CredentialName)];
        if (!string.IsNullOrEmpty(credentialName))
            this.Credentials = SecureCredentials.TryCreate(credentialName, context) as GiteaAccount;

        var resourceName = config["ResourceName"];
        if (!string.IsNullOrEmpty(resourceName))
            this.Resource = SecureResource.TryCreate(SecureResourceType.GitRepository, resourceName, context) as GiteaRepository;

        if (this.Credentials == null && this.Resource != null)
            this.Credentials = this.Resource.GetCredentials(context) as GiteaAccount;

        var groupName = AH.CoalesceString(config["OrganizationName"], this.Resource?.OrganizationName);

        if (groupName == null || this.Credentials == null || string.IsNullOrEmpty(this.Credentials.ServiceUrl))
            yield break;

        this.ComponentConfiguration = config;

        this.Client = new GiteaClient(
            this.Credentials.ServiceUrl,
            string.IsNullOrEmpty(config["Password"])
                ? AH.Unprotect(this.Credentials.Password)
                : config["Password"]
        );

        var suggestions = this.GetSuggestionsAsync(cancellationToken);
        if (suggestions != null)
        {
            await foreach (var s in suggestions.ConfigureAwait(false))
            {
                if (string.IsNullOrEmpty(startsWith) || s.StartsWith(startsWith, StringComparison.OrdinalIgnoreCase))
                    yield return s;
            }
        }
    }

    protected abstract IAsyncEnumerable<string>? GetSuggestionsAsync(CancellationToken cancellationToken);
}
