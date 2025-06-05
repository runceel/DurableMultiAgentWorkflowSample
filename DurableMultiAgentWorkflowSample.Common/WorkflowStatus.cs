using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json.Serialization;

namespace DurableMultiAgentWorkflowSample.Common;
/// <summary>
/// Represents the status of a workflow.
/// </summary>
public record WorkflowStatus(
    string Id,
    WorkflowStatusType Status,
    string[] CurrentAgents,
    ChatMessageContent? FinalResult,
    ChatHistory? ChatHistory);

/// <summary>
/// Defines the possible states of a workflow.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WorkflowStatusType
{
    /// <summary>
    /// The workflow is orchestrating.
    /// </summary>
    Orchestrating,
    /// <summary>
    /// The workflow is currently invoking an agent.
    /// </summary>
    InvokingAgent,
    /// <summary>
    /// The workflow has completed successfully.
    /// </summary>
    Completed,
    /// <summary>
    /// The workflow has failed.
    /// </summary>
    Failed,
    /// <summary>
    /// The workflow is waiting for external input.
    /// </summary>
    WaitingForInput
}
