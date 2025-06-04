using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Caching.Distributed;
using DurableMultiAgentWorkflowSample.Workflow.Workflows;

namespace DurableMultiAgentWorkflowSample.Workflow.Workflows.Activities;

public class InvokeAgentActivity(IServiceProvider services, IDistributedCache cache)
{
    /// <summary>
    /// Invokes the specified agent to process a chat history and returns the resulting message content.
    /// </summary>
    /// <remarks>This method creates a thread for the provided chat history, invokes the specified agent to
    /// process it,  and ensures that the thread is deleted after processing, regardless of success or
    /// failure.</remarks>
    /// <param name="input">A tuple containing the agent's name and the chat history to be processed.  <paramref name="input.agentName"/>
    /// specifies the name of the agent to invoke,  and <paramref name="input.messages"/> represents the chat history to
    /// be processed.</param>
    /// <returns>A <see cref="ChatMessageContent"/> object representing the result of the agent's processing.</returns>
    [Function(nameof(InvokeAgentActivity))]
    public async Task<(ChatMessageContent Message, ChatHistory ChatHistory)> RunAsync(
        [ActivityTrigger] InvokeAgentRequest input)
    {
        var (agentName, message, instanceId, chatHistory) = input;
        
        // Save workflow status before agent invocation
        var beforeStatus = new WorkflowStatus(instanceId, WorkflowStatusType.InvokingAgent, [agentName], null, chatHistory);
        await cache.SetAsync(instanceId, 
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(beforeStatus), 
            new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromHours(1)
            }).ConfigureAwait(false);
        
        var agent = services.GetRequiredKeyedService<Agent>(agentName);
        AgentThread? thread = null;
        try
        {
            var result = await agent.InvokeAsync(message, thread).FirstAsync();
            thread = result.Thread;
            
            // Add the result to chat history
            chatHistory.Add(result.Message);
            
            // Save workflow status after agent invocation
            var afterStatus = new WorkflowStatus(instanceId, WorkflowStatusType.Orchestrating, [], null, chatHistory);
            await cache.SetAsync(instanceId, 
                System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(afterStatus), 
                new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromHours(1)
                }).ConfigureAwait(false);
            
            return (result.Message, chatHistory);
        }
        finally
        {
            if (thread != null)
            {
                await thread.DeleteAsync();
            }
        }
    }
}

public record InvokeAgentRequest(string AgentName, ChatMessageContent Message, string InstanceId, ChatHistory ChatHistory);
