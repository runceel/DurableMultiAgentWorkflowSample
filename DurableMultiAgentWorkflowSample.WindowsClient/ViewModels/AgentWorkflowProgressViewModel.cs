using DurableMultiAgentWorkflowSample.Common;

namespace DurableMultiAgentWorkflowSample.WindowsClient.ViewModels;
public class AgentWorkflowProgressViewModel(AgentWorkflowProgress agentWorkflowProgress)
{
    public string Text => agentWorkflowProgress.Status switch
    {
        WorkflowStatusType.Orchestrating => $"Orchestrating workflow... Last message: {agentWorkflowProgress.ChatMessage?.AuthorName}: {agentWorkflowProgress.ChatMessage?.Content}",
        WorkflowStatusType.InvokingAgent => $"Invoking agent(s): {string.Join(", ", agentWorkflowProgress.AgentNames ?? Array.Empty<string>())}",
        WorkflowStatusType.Completed => $"Workflow completed successfully. The final result: {agentWorkflowProgress.ChatMessage?.AuthorName}: {agentWorkflowProgress.ChatMessage?.Content}",
        WorkflowStatusType.Failed => "Workflow failed.",
        WorkflowStatusType.WaitingForInput => $"Waiting for input. Ask: {agentWorkflowProgress.ChatMessage?.AuthorName}: {agentWorkflowProgress.ChatMessage?.Content}",
        _ => "Unknown status."
    };
}
