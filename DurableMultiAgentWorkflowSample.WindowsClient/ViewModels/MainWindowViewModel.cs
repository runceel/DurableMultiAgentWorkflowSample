#pragma warning disable SKEXP0001
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DurableMultiAgentWorkflowSample.ClientLibrary;
using DurableMultiAgentWorkflowSample.Common;
using Microsoft.Extensions.Logging;

namespace DurableMultiAgentWorkflowSample.WindowsClient.ViewModels;
public partial class MainWindowViewModel(IAgentWorkflowClient agentWorkflowClient, ILogger<MainWindowViewModel> logger) : ObservableObject
{
    [ObservableProperty]
    private string _message = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartWorkflowCommand))]
    private bool _isRunning = false;

    public bool CanExecuteStartWorkflow() => !IsRunning;

    [RelayCommand(CanExecute = nameof(CanExecuteStartWorkflow), IncludeCancelCommand = true)]
    public async Task StartWorkflowAsync(CancellationToken cancellationToken)
    {
        IsRunning = true;
        try
        {
            var progress = new Progress<AgentWorkflowProgress>(UpdateProgress);
            await agentWorkflowClient.StartWorkflowAsync(Message, progress);
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., show a message to the user)
            Console.WriteLine($"Error starting workflow: {ex.Message}");
        }
        finally
        {
            IsRunning = false;
        }
    }

    private void UpdateProgress(AgentWorkflowProgress progress)
    {
        // Update the UI or handle the progress update as needed
        logger.LogInformation("Progress: {status} - {authorName}({role}) {content}", 
            progress.Status, 
            progress.ChatMessage.AuthorName, 
            progress.ChatMessage.Role, 
            progress.ChatMessage.Content);
    }
}
