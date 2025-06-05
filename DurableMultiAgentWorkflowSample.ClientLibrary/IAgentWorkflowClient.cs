using DurableMultiAgentWorkflowSample.Common;

namespace DurableMultiAgentWorkflowSample.ClientLibrary;
public interface IAgentWorkflowClient
{
    Task StartWorkflowAsync(
        string initialMessage, 
        IProgress<AgentWorkflowProgress> progress, 
        CancellationToken cancellationToken = default);
}

