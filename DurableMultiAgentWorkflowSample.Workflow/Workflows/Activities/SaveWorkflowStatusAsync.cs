using DurableMultiAgentWorkflowSample.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DurableMultiAgentWorkflowSample.Workflow.Workflows.Activities;
public class SaveWorkflowStatusAsync(IDistributedCache cache)
{
    [Function(nameof(SaveWorkflowStatusAsync))]
    public async Task RunAsync(
        [ActivityTrigger] WorkflowStatus status)
    {
        await cache.SetAsync(status.Id, 
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(status), 
            new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromHours(1) // Set expiration as needed
            }).ConfigureAwait(false);
    }
}
