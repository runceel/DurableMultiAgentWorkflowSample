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
    public static async Task<ChatMessageContent> RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        ILogger logger = context.CreateReplaySafeLogger(nameof(AgentOrchestrator));
        var chatHistory = context.GetInput<ChatHistory>();
        if (chatHistory == null || chatHistory is { Count: 0 })
        {
            logger.LogError("No input message provided to the orchestrator.");
            return new ChatMessageContent(AuthorRole.Assistant,
                "Please provide a valid input message.");
        }

        if (chatHistory.Count >= 100)
        {
            logger.LogError("Chat history exceeds the maximum allowed length of 100 messages.");
            return new ChatMessageContent(AuthorRole.Assistant,
                "Chat history exceeds the maximum allowed length of 100 messages. Please start a new conversation.");
        }

        try
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
                (var humanResponse, chatHistory) = await context.AskToHumanAsync(askToHumanMessage, chatHistory);
                if (humanResponse == null)
                {
                    return new ChatMessageContent(AuthorRole.Assistant, "No response received from the human user.");
                }

                if (humanResponse.Content?.Equals("Approve", StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    await context.SaveWorkflowStatusAsync(WorkflowStatusType.Completed, chatHistory, writerResponse);
                    return writerResponse;
                }
                else
                {
                    logger.LogInformation("Human user requested revisions.");
                    reviewerResponse = new ChatMessageContent(AuthorRole.User, humanResponse.Content);
                    chatHistory.Add(reviewerResponse);
                    await context.SaveWorkflowStatusAsync(WorkflowStatusType.Orchestrating, chatHistory);
                }
            }

            chatHistory.Add(new ChatMessageContent(AuthorRole.Assistant,
                $"""
                ## Writer Output:
                {writerResponse.Content}

                ## Review Result:
                {reviewerResponse.Content}

                ## Request:
                Please rewrite the Writer Output based on the review result.
                """));
            await context.SaveWorkflowStatusAsync(WorkflowStatusType.Orchestrating, chatHistory);

            context.ContinueAsNew(chatHistory);
            return null!;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during the orchestration process.");
            return new ChatMessageContent(AuthorRole.Assistant,
                "An unexpected error occurred. Please create a new conversation. If the issue persists, contact support.");
        }
    }
}
