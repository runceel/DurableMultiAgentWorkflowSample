using DurableMultiAgentWorkflowSample.Workflow.Agents.Activities;
using Microsoft.DurableTask;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace DurableMultiAgentWorkflowSample.Workflow;
internal static class TaskOrchestrationContextExtensions
{
    public static Task<ChatMessageContent> InvokeAgentAsync(
        this TaskOrchestrationContext context,
        string agentName,
        ChatHistory messages)
    {
        return context.CallActivityAsync<ChatMessageContent>(
            nameof(InvokeAgentActivity),
            (agentName, messages));
    }

    public static async Task<ChatMessageContent> AskToHumanAsync(
        this TaskOrchestrationContext context,
        ChatHistory messages,
        TimeSpan? timeout = null)
    {
        timeout = timeout ?? TimeSpan.FromDays(3);
        context.SetCustomStatus();
        var response = await context.WaitForExternalEvent<ChatMessageContent>(
            "Reply",
            timeout.Value);
        return response;
    }
}

