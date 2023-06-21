using System.Text.Json.Serialization;

namespace Inedo.Extensions.Gitea.Clients;

[JsonSerializable(typeof(BranchInfo[]))]
[JsonSerializable(typeof(PullRequestInfo[]))]
[JsonSerializable(typeof(RepositoryInfo))]
[JsonSerializable(typeof(OrganizationOrRepoInfo[]))]
[JsonSerializable(typeof(IssueInfo[]))]
[JsonSerializable(typeof(MilestoneInfo[]))]
[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class GiteaJsonContext : JsonSerializerContext
{
}
