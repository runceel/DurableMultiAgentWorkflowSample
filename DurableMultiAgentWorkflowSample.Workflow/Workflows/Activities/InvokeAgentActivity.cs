using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel;

namespace DurableMultiAgentWorkflowSample.Workflow.Workflows.Activities;

public class InvokeAgentActivity(IServiceProvider services)
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
    public async Task<ChatMessageContent> RunAsync(
        [ActivityTrigger] InvokeAgentRequest input)
    {
        var (agentName, messages) = input;
        var agent = services.GetRequiredKeyedService<Agent>(agentName);
        AgentThread? thread = null;
        try
        {
            var result = await agent.InvokeAsync(messages, thread).FirstAsync();
            thread = result.Thread;
            return result.Message;
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

public record InvokeAgentRequest(string AgentName, ChatHistory Messages);
