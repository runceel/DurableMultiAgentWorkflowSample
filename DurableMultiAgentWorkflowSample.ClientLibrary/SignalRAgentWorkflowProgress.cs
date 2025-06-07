using System.Net.Http.Json;
using System.Text.Json;
using DurableMultiAgentWorkflowSample.Common;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;

namespace DurableMultiAgentWorkflowSample.ClientLibrary;
public class SignalRAgentWorkflowProgress(
    HttpClient httpClient, 
    IConfiguration configuration, 
    JsonSerializerOptions? jsonSerializerOptions = null) : IAgentWorkflowClient
{
    public async Task ReplyAsync(ReplyRequest replyRequest, CancellationToken cancellationToken = default)
    {
        if (httpClient.BaseAddress == null)
        {
            throw new InvalidOperationException("HttpClient BaseAddress must be set before sending human input.");
        }

        var jsonOptions = jsonSerializerOptions ?? DefaultJsonSerializerOptions.Value;
        await httpClient.PostAsJsonAsync("api/Reply", replyRequest, jsonOptions, cancellationToken);
    }

    public async Task StartWorkflowAsync(StartRequest startRequest, IProgress<AgentWorkflowProgress> progress, CancellationToken cancellationToken = default)
    {
        if (httpClient.BaseAddress == null)
        {
            throw new InvalidOperationException("HttpClient BaseAddress must be set before starting the workflow.");
        }

        var jsonOptions = jsonSerializerOptions ?? DefaultJsonSerializerOptions.Value;

        var realBaseAddress = ResolveRealBaseAddress();
        var hubConnection = new HubConnectionBuilder()
            .WithUrl(new Uri(new(realBaseAddress), $"/api?userId={startRequest.Id}"))
            .WithAutomaticReconnect()
            .Build();
        hubConnection.On(
            nameof(AgentWorkflowProgress),
            (AgentWorkflowProgress progressUpdate) => progress.Report(progressUpdate));
        await hubConnection.StartAsync(cancellationToken);

        try
        {
            var startResponse = await httpClient.PostAsJsonAsync("api/Start",
                startRequest,
                jsonOptions,
                cancellationToken);
            startResponse.EnsureSuccessStatusCode();

            var workflowInfo = await startResponse.Content.ReadFromJsonAsync<WorkflowInfo>(jsonOptions, cancellationToken);
            if (workflowInfo == null)
            {
                throw new InvalidOperationException("Failed to start workflow: no workflow info returned.");
            }
        }
        catch
        {
            await hubConnection.DisposeAsync();
            throw;
        }
    }

    private string ResolveRealBaseAddress()
    {
        if (httpClient.BaseAddress == null)
        {
            throw new InvalidOperationException("HttpClient BaseAddress must be set before resolving the real base address.");
        }

        var schemas = httpClient.BaseAddress.Scheme.Split('+');
        foreach (var schema in schemas)
        {
            var realBaseAddress = configuration[$"services:{httpClient.BaseAddress.Host}:{schema}:0"];
            if (realBaseAddress != null)
            {
                return realBaseAddress;
            }
        }

        throw new InvalidOperationException($"Real base address not found for {httpClient.BaseAddress}.");
    }
}
