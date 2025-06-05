using DurableMultiAgentWorkflowSample.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Net;
using System.Text.Json;

namespace DurableMultiAgentWorkflowSample.Workflow.Workflows;
public class WorkflowManageFunctions(IDistributedCache cache)
{
    [Function(nameof(Start))]
    public async Task<HttpResponseData> Start(
        [HttpTrigger("post")] HttpRequestData request,
        [DurableClient] DurableTaskClient client)
    {
        var initialMessage = await JsonSerializer.DeserializeAsync<StartRequest>(request.Body);
        if (initialMessage == null || string.IsNullOrEmpty(initialMessage.Message))
        {
            var batRequestResponse = request.CreateResponse(HttpStatusCode.BadRequest);
            await batRequestResponse.WriteStringAsync("Initial message is required.");
            return batRequestResponse;
        }

        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(AgentOrchestrator),
            new ChatHistory { new(AuthorRole.User, initialMessage.Message) });
        var response = request.CreateResponse(HttpStatusCode.Accepted);
        response.Headers.Add("Content-Type", "application/json");
        await JsonSerializer.SerializeAsync(response.Body, new WorkflowInfo(instanceId));
        return response;
    }

    [Function(nameof(GetWorkflowStatus))]
    public async Task<IActionResult> GetWorkflowStatus(
        [HttpTrigger("get")] HttpRequest request,
        [DurableClient] DurableTaskClient client)
    {
        string? instanceId = request.Query["instanceId"];
        if (string.IsNullOrEmpty(instanceId))
        {
            return new BadRequestObjectResult("Instance ID is required.");
        }

        var status = await cache.GetAsync(instanceId);
        if (status == null)
        {
            return new NotFoundObjectResult($"Workflow with ID {instanceId} not found.");
        }
        return new OkObjectResult(JsonSerializer.Deserialize<WorkflowStatus>(status));
    }

    [Function(nameof(Reply))]
    public async Task<IActionResult> Reply(
        [HttpTrigger("post")] HttpRequest request,
        [DurableClient] DurableTaskClient client)
    {
        var requestBody = await request.ReadFromJsonAsync<ReplyRequest>();
        if (requestBody == null)
        {
            return new BadRequestObjectResult("Reply message is required.");
        }

        await client.RaiseEventAsync(requestBody.Id, OrchestratorEventNames.Reply, requestBody.Message);
        return new OkResult();
    }
}

public record ReplyRequest(string Id, string Message);
