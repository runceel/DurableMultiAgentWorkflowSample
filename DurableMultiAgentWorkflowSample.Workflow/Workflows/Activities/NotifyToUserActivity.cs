using DurableMultiAgentWorkflowSample.Common;
using Microsoft.Azure.Functions.Worker;

namespace DurableMultiAgentWorkflowSample.Workflow.Workflows.Activities;

public class NotifyToUserActivity
{
    [Function(nameof(NotifyToUserActivity))]
    [SignalROutput(HubName = NotificationFunctions.HubName)]
    public SignalRMessageAction Run(
        [ActivityTrigger] NotifyToUserRequest input)
    {
        var (userId, progress) = input;
        return new(nameof(AgentWorkflowProgress), [progress])
        {
            UserId = userId,
        };
    }
}

public record NotifyToUserRequest(string UserId, AgentWorkflowProgress Progress);
