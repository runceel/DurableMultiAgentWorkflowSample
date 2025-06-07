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
        var startRequest = await JsonSerializer.DeserializeAsync<StartRequest>(request.Body, DefaultJsonSerializerOptions.Value);
        if (startRequest == null || string.IsNullOrEmpty(startRequest.Message))
        {
            var batRequestResponse = request.CreateResponse(HttpStatusCode.BadRequest);
            await batRequestResponse.WriteStringAsync("Initial message is required.");
            return batRequestResponse;
        }

        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(AgentOrchestrator),
            new ChatHistory { new(AuthorRole.User, startRequest.Message) },
            options: new(startRequest.Id));
        var response = request.CreateResponse(HttpStatusCode.Accepted);
        response.Headers.Add("Content-Type", "application/json");
        await JsonSerializer.SerializeAsync(response.Body, new WorkflowInfo(instanceId), DefaultJsonSerializerOptions.Value);
        
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
        return new OkObjectResult(JsonSerializer.Deserialize<WorkflowStatus>(status, DefaultJsonSerializerOptions.Value));
    }

    [Function(nameof(Reply))]
    public async Task<IActionResult> Reply(
        [HttpTrigger("post")] HttpRequest request,
        [DurableClient] DurableTaskClient client)
    {
        var requestBody = await request.ReadFromJsonAsync<ReplyRequest>(DefaultJsonSerializerOptions.Value);
        if (requestBody == null)
        {
            return new BadRequestObjectResult("Reply message is required.");
        }

        await client.RaiseEventAsync(requestBody.Id, OrchestratorEventNames.Reply, requestBody.Message);
        return new OkResult();
    }
}
