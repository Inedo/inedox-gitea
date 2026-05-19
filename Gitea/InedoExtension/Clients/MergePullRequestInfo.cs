using System.Text.Json.Serialization;

namespace Inedo.Extensions.Gitea.Clients;

internal sealed class MergePullRequestInfo(string @do, string? mergeMessageField, string? mergeTitleField, string? headCommitId)
{
    [JsonPropertyName(nameof(Do))]
    public string Do { get; } = @do;
    [JsonPropertyName(nameof(MergeMessageField))]
    public string? MergeMessageField { get; } = mergeMessageField;
    [JsonPropertyName(nameof(MergeTitleField))]
    public string? MergeTitleField { get; } = mergeTitleField;
    public string? HeadCommitId { get; } = headCommitId;
}
