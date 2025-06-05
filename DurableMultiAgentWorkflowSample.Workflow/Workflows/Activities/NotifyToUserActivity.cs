using Microsoft.Azure.Functions.Worker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DurableMultiAgentWorkflowSample.Workflow.Workflows.Activities;
public class NotifyToUserActivity
{
    [Function(nameof(NotifyToUserActivity))]
    public async Task RunAsync(
        [ActivityTrigger] NotifyToUserRequest input)
    {
        var (userId, message) = input;
        // Here you would implement the logic to notify the user, e.g., send an email or a push notification.
        // For demonstration purposes, we will just simulate a delay.
        await Task.Delay(1000); // Simulate some work
        Console.WriteLine($"Notification sent to user {userId}: {message}");
    }
}

public record NotifyToUserRequest(string UserId, string Message);
