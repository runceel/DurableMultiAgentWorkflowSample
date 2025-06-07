using System.Windows;
using DurableMultiAgentWorkflowSample.WindowsClient.ViewModels;

namespace DurableMultiAgentWorkflowSample.WindowsClient;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(MainWindowViewModel mainWindowViewModel)
    {
        InitializeComponent();
        DataContext = mainWindowViewModel;
    }
}
