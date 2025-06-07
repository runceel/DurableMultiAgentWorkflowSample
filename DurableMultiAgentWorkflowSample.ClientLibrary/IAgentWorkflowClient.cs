using DurableMultiAgentWorkflowSample.Common;

namespace DurableMultiAgentWorkflowSample.ClientLibrary;
public interface IAgentWorkflowClient
{
    Task StartWorkflowAsync(
        StartRequest startRequest, 
        IProgress<AgentWorkflowProgress> progress, 
        CancellationToken cancellationToken = default);

    Task ReplyAsync(ReplyRequest replyRequest, CancellationToken cancellationToken = default);
}
