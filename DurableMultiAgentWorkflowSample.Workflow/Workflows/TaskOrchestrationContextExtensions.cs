#pragma warning disable SKEXP0001
using DurableMultiAgentWorkflowSample.Common;
using DurableMultiAgentWorkflowSample.Workflow.Workflows.Activities;
using Microsoft.DurableTask;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

namespace DurableMultiAgentWorkflowSample.Workflow.Workflows;
internal static class TaskOrchestrationContextExtensions
{
    private static readonly TaskOptions s_defaultTaskOptions = new()
    {
        Retry = new(new RetryPolicy(
            3, 
            TimeSpan.FromSeconds(1),
            maxRetryInterval: TimeSpan.FromSeconds(10))),
    };
    public static async Task<AgentResult> InvokeAgentAsync(
        this TaskOrchestrationContext context,
        string agentName,
        ChatHistory chatHistory)
    {
        await Task.WhenAll(
            context.SaveWorkflowStatusAsync(WorkflowStatusType.InvokingAgent, chatHistory, agentName),
            context.NotifyToUserAsync(new(WorkflowStatusType.InvokingAgent, [agentName])));
        var result = await context.CallActivityAsync<ChatMessageContent>(
            nameof(InvokeAgentActivity),
            new InvokeAgentRequest(agentName, chatHistory),
            s_defaultTaskOptions);
        chatHistory.Add(result);
        await Task.WhenAll(
            context.SaveWorkflowStatusAsync(WorkflowStatusType.Orchestrating, chatHistory),
            context.NotifyToUserAsync(new(WorkflowStatusType.Orchestrating, [], result.ToAgentMessage())));
        return new(result, chatHistory);
    }

    public static async Task<AgentResult> AskToHumanAsync(
        this TaskOrchestrationContext context,
        ChatMessageContent message,
        ChatHistory chatHistory,
        TimeSpan? timeout = null)
    {
        timeout = timeout ?? TimeSpan.FromDays(3);
        chatHistory.Add(message);
        await context.SaveAndNotifyAsync(WorkflowStatusType.WaitingForInput, chatHistory, message);
        var result = await context.WaitForExternalEvent<string>(
            OrchestratorEventNames.Reply,
            timeout.Value);
        var userMessage = new ChatMessageContent(AuthorRole.User, result);
        chatHistory.Add(userMessage);
        await context.SaveWorkflowStatusAsync(WorkflowStatusType.Orchestrating, chatHistory);
        return new(userMessage, chatHistory);
    }

    public static async Task NotifyToUserAsync(
        this TaskOrchestrationContext context,
        AgentWorkflowProgress progress)
    {
        await context.CallActivityAsync(
            nameof(NotifyToUserActivity), 
            new NotifyToUserRequest(context.InstanceId, progress), 
            s_defaultTaskOptions);
    }

    public static async Task SaveAndNotifyAsync(
        this TaskOrchestrationContext context,
        WorkflowStatusType statusType,
        ChatHistory chatHistory,
        ChatMessageContent? message = null,
        params string[] currentAgents)
    {
        var finalMessage = statusType == WorkflowStatusType.Completed || statusType == WorkflowStatusType.Failed ? message : null; 
        await Task.WhenAll(
            context.SaveWorkflowStatusAsync(
                statusType, 
                chatHistory, 
                finalMessage, 
                currentAgents),
            context.NotifyToUserAsync(new(statusType, currentAgents, message?.ToAgentMessage())));
    }

    public static async Task SaveWorkflowStatusAsync(
        this TaskOrchestrationContext context,
        WorkflowStatusType statusType,
        ChatHistory chatHistory,
        params string[] currentAgents)
    {
        await SaveWorkflowStatusAsync(
            context,
            statusType,
            chatHistory,
            null,
            currentAgents);
    }

    public static async Task SaveWorkflowStatusAsync(
        this TaskOrchestrationContext context,
        WorkflowStatusType statusType,
        ChatHistory? chatHistory,
        ChatMessageContent? finalResult,
        params string[] currentAgents)
    {
        var status = new WorkflowStatus(context.InstanceId, statusType, currentAgents, finalResult, chatHistory);
        await context.CallActivityAsync(nameof(SaveWorkflowStatusAsync), status, s_defaultTaskOptions);
    }
}

public record AgentResult(
    ChatMessageContent Message,
    ChatHistory ChatHistory)
{
    public T? GetValue<T>()
    {
        if (string.IsNullOrEmpty(Message.Content))
        {
            return default;
        }
        return JsonSerializer.Deserialize<T>(Message.Content);
    }
}

public class OrchestratorEventNames
{
    public const string Reply = "Reply";
}
