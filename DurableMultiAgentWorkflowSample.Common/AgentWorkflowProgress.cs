using Microsoft.SemanticKernel;

namespace DurableMultiAgentWorkflowSample.Common;
public record class AgentWorkflowProgress(
    WorkflowStatusType Status,
    ChatMessageContent ChatMessage,
    string? ErrorMessage = null
);
