using System.Text.Json.Serialization;

namespace Inedo.Extensions.Gitea.Clients;

internal sealed class MergePullRequestInfo
{
    public MergePullRequestInfo(string @do, string? mergeMessageField, string? mergeTitleField, string? headCommitId)
    {
        this.Do = @do;
        this.MergeMessageField = mergeMessageField;
        this.MergeTitleField = mergeTitleField;
        this.HeadCommitId = headCommitId;
    }

    public string Do { get; }
    public string? MergeMessageField { get; }
    public string? MergeTitleField { get; }
    [JsonPropertyName("head_commit_id")]
    public string? HeadCommitId { get; }
}
