#pragma warning disable SKEXP0001
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Data;
using System.Windows.Documents;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DurableMultiAgentWorkflowSample.ClientLibrary;
using DurableMultiAgentWorkflowSample.Common;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace DurableMultiAgentWorkflowSample.WindowsClient.ViewModels;
public partial class MainWindowViewModel : ObservableObject
{
    private readonly IAgentWorkflowClient _agentWorkflowClient;
    private readonly ILogger<MainWindowViewModel> _logger;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendCommand))]
    private string _message = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    private string? _id;

    [ObservableProperty]
    private bool _isRunning = false;

    public MainWindowViewModel(IAgentWorkflowClient agentWorkflowClient, ILogger<MainWindowViewModel> logger)
    {
        _agentWorkflowClient = agentWorkflowClient;
        _logger = logger;
        BindingOperations.EnableCollectionSynchronization(Items, new object());
    }

    // ListBoxにバインドするコレクション
    public ObservableCollection<AgentWorkflowProgressViewModel> Items { get; } = new();

    [RelayCommand(CanExecute = nameof(CanExecuteSend))]
    public async Task SendAsync()
    {
        IsRunning = true;
        try
        {
            if (Id == null)
            {
                await StartWorkflowAsync();
            }
            else
            {
                await ReplyAsync();
            }

            Message = "";
        }
        finally
        {
            IsRunning = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteCancel))]
    public void Cancel()
    {
        Id = null;
    }

    private bool CanExecuteCancel() => Id != null;


    private bool CanExecuteSend() => !IsRunning && !string.IsNullOrWhiteSpace(Message);

    private async Task StartWorkflowAsync()
    {
        try
        {
            Items.Clear();
            var progress = new Progress<AgentWorkflowProgress>(UpdateProgress);
            Id = Guid.NewGuid().ToString();
            await _agentWorkflowClient.StartWorkflowAsync(new(Id, Message), progress);
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., show a message to the user)
            _logger.LogError(ex, $"Error starting workflow: {ex.Message}");
        }
        finally
        {
            IsRunning = false;
        }
    }

    private async Task ReplyAsync()
    {
        if (string.IsNullOrEmpty(Id) || string.IsNullOrEmpty(Message))
        {
            _logger.LogWarning("Cannot reply: Id or Message is null or empty.");
            return;
        }

        await _agentWorkflowClient.ReplyAsync(new(Id, Message));
    }


    private void UpdateProgress(AgentWorkflowProgress progress)
    {
        // Update the UI or handle the progress update as needed
        _logger.LogInformation("Progress: {json}", 
            JsonSerializer.Serialize(progress, DefaultJsonSerializerOptions.Value));
        Items.Add(new(progress));
    }
}
