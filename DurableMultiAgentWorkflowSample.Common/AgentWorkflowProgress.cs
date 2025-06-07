#pragma warning disable SKEXP0001 
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace DurableMultiAgentWorkflowSample.Common;
public record class AgentWorkflowProgress(
    WorkflowStatusType Status,
    string[]? AgentNames = null,
    AgentMessage? ChatMessage = null);

public record AgentMessage(
    AuthorRole Role,
    string? AuthorName,
    string? Content);

public static class AgentMessageExtensions
{
    public static AgentMessage ToAgentMessage(this ChatMessageContent chatMessageContent) =>
        new AgentMessage(
            chatMessageContent.Role,
            chatMessageContent.AuthorName,
            chatMessageContent.Content);
}

