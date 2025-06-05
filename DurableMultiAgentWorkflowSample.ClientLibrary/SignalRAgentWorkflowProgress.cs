using System.Net.Http.Json;
using DurableMultiAgentWorkflowSample.Common;
using Microsoft.AspNetCore.SignalR.Client;

namespace DurableMultiAgentWorkflowSample.ClientLibrary;
public class SignalRAgentWorkflowProgress(HttpClient httpClient) : IAgentWorkflowClient
{
    public async Task StartWorkflowAsync(string initialMessage, IProgress<AgentWorkflowProgress> progress, CancellationToken cancellationToken = default)
    {
        if (httpClient.BaseAddress == null)
        {
            throw new InvalidOperationException("HttpClient BaseAddress must be set before starting the workflow.");
        }

        var startResponse = await httpClient.PostAsJsonAsync("/api/Start", new StartRequest(initialMessage), cancellationToken);
        startResponse.EnsureSuccessStatusCode();

        var workflowInfo = await startResponse.Content.ReadFromJsonAsync<WorkflowInfo>(cancellationToken);
        if (workflowInfo == null)
        {
            throw new InvalidOperationException("Failed to start workflow: no workflow info returned.");
        }

        var hubConnection = new HubConnectionBuilder()
            .WithUrl(new Uri(httpClient.BaseAddress, $"/api?userId={workflowInfo.Id}"))
            .WithAutomaticReconnect()
            .Build();
        hubConnection.On(
            nameof(AgentWorkflowProgress), 
            (AgentWorkflowProgress progressUpdate) => progress.Report(progressUpdate));
        await hubConnection.StartAsync(cancellationToken);
    }
}
