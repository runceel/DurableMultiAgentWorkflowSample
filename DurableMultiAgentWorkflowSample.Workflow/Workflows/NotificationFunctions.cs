using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace DurableMultiAgentWorkflowSample.Workflow.Workflows;

public class NotificationFunctions(ILogger<NotificationFunctions> logger)
{
    public const string HubName = "NotifyHub";

    [Function("negotiate")]
    public HttpResponseData Negotiate(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
        [SignalRConnectionInfoInput(HubName = HubName, UserId = "{query.userId}")] MyConnectionInfo connectionInfo)
    {
        logger.LogInformation("SignalR Connection URL = '{url}'", connectionInfo.Url);

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        response.WriteString($"Connection URL = '{connectionInfo.Url}'");
        
        return response;
    }
}

public class MyConnectionInfo
{
    public required string Url { get; set; }

    public required string AccessToken { get; set; }
}
