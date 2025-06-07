using System.Collections.Generic;
using System.Net;
using DurableMultiAgentWorkflowSample.Common;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.SignalRService;
using Microsoft.Extensions.Logging;

namespace DurableMultiAgentWorkflowSample.Workflow.Workflows.Activities;

[SignalRConnection]
public class NotifyToUserActivity(
    IServiceProvider services,
    ILogger<NotifyToUserActivity> logger) : ServerlessHub(services)
{
    public const string HubName = "NotifyHub";

    [Function("negotiate")]
    public async Task<HttpResponseData> Negotiate(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        var userId = req.Query["userId"];
        if (string.IsNullOrEmpty(userId))
        {
            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteStringAsync("User ID is required.");
            return response;
        }

        var negotiateResponse = await NegotiateAsync(new() { UserId = userId });
        var responseData = req.CreateResponse(HttpStatusCode.OK);
        await responseData.WriteBytesAsync(negotiateResponse.ToArray());
        return responseData;
    }

    [Function(nameof(NotifyToUserActivity))]
    public async Task Run(
        [ActivityTrigger] NotifyToUserRequest input)
    {
        var (userId, progress) = input;
        try
        {
            await Clients.User(userId).SendAsync(nameof(AgentWorkflowProgress), progress);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to notify user {UserId} with progress {Progress}", userId, progress);
            throw;
        }
    }

}

public record NotifyToUserRequest(string UserId, AgentWorkflowProgress Progress);
