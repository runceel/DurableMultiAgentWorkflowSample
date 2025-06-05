using DurableMultiAgentWorkflowSample.Common;
using DurableMultiAgentWorkflowSample.Workflow.Agents;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace DurableMultiAgentWorkflowSample.Workflow.Workflows;

public static class AgentOrchestrator
{
    [Function(nameof(AgentOrchestrator))]
    public static async Task RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        ILogger logger = context.CreateReplaySafeLogger(nameof(AgentOrchestrator));
        var chatHistory = context.GetInput<ChatHistory>();
        if (chatHistory == null || chatHistory is { Count: 0 })
        {
            logger.LogError("No input message provided to the orchestrator.");
            await context.SaveWorkflowStatusAsync(
                WorkflowStatusType.Failed, 
                chatHistory,
                new ChatMessageContent(AuthorRole.Assistant, "No input message provided to the orchestrator."));
            return;
        }

        if (chatHistory.Count >= 100)
        {
            logger.LogError("Chat history exceeds the maximum allowed length of 100 messages.");
            await context.SaveWorkflowStatusAsync(
                WorkflowStatusType.Failed, 
                chatHistory,
                new ChatMessageContent(AuthorRole.Assistant,
                "Chat history exceeds the maximum allowed length of 100 messages. Please start a new conversation."));
            return;
        }

        try
        {
            WorkflowStatusType status = WorkflowStatusType.Orchestrating;
            while (status == WorkflowStatusType.Orchestrating)
            {
                (status, chatHistory) = await AdvanceWorkflowTurnAsync(context, chatHistory, logger);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during the orchestration process.");
            await context.SaveWorkflowStatusAsync(WorkflowStatusType.Failed, chatHistory,
                new ChatMessageContent(AuthorRole.Assistant,
                "An unexpected error occurred. Please create a new conversation. If the issue persists, contact support."));
        }
    }

    private static async Task<(WorkflowStatusType Status, ChatHistory ChatHistory)> AdvanceWorkflowTurnAsync(
        TaskOrchestrationContext context,
        ChatHistory chatHistory,
        ILogger logger)
    {
        // Write the content
        (var writerResponse, chatHistory) = await context.InvokeAgentAsync(AgentDefinitions.WriterAgentName, chatHistory);

        // Review the content
        (var reviewerResponse, chatHistory) = await context.InvokeAgentAsync(AgentDefinitions.ReviewerAgentName, chatHistory);

        // Check if the reviewer response indicates approval or requires further action
        var response = await context.InvokeAgentAsync(AgentDefinitions.ApproverAgentName, chatHistory);
        var isApproved = response.GetValue<ApproverAgentResponse>();
        chatHistory = response.ChatHistory;

        if (isApproved?.Approved == true)
        {
            // Content is approved by the approver agent
            // Loop in to human for final approval
            var askToHumanMessage = new ChatMessageContent(AuthorRole.Assistant, $"""
                    The content has been approved.

                    ## Content:
                    {writerResponse.Content}

                    ## Request
                    If this content is acceptable, please reply with "Approve". If revisions are needed, please specify the changes required.
                    """);
            (reviewerResponse, chatHistory) = await context.AskToHumanAsync(askToHumanMessage, chatHistory);
            if (reviewerResponse == null)
            {
                chatHistory.Add(new ChatMessageContent(AuthorRole.Assistant, "No response received from the human user."));
                return (WorkflowStatusType.Failed, chatHistory);
            }

            if (reviewerResponse.Content?.Equals("Approve", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                await context.SaveWorkflowStatusAsync(WorkflowStatusType.Completed, chatHistory, writerResponse);
                return (WorkflowStatusType.Completed, chatHistory);
            }
        }

        // Generate a new message for the next turn
        chatHistory.AddAssistantMessage($"""
            Rewrite a new content based on the reviewer's feedback.

            ## Original Content:
            {writerResponse.Content}

            ## Reviewer's Feedback:
            {reviewerResponse.Content}
            """);

        return (WorkflowStatusType.Orchestrating, chatHistory);
    }
}
