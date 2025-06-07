using DurableMultiAgentWorkflowSample.ClientLibrary;
using DurableMultiAgentWorkflowSample.WindowsClient.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DurableMultiAgentWorkflowSample.WindowsClient;
internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.AddAppDefaults();

        builder.Services.AddSingleton<App>();
        builder.Services.AddSingleton<MainWindow>();
        builder.Services.AddSingleton<MainWindowViewModel>();

        builder.Services.AddHttpClient<IAgentWorkflowClient, SignalRAgentWorkflowProgress>(client =>
        {
            client.BaseAddress = new("https+http://workflow");
        });


        var appHost = builder.Build();
        var app = appHost.Services.GetRequiredService<App>();
        var mainWindow = appHost.Services.GetRequiredService<MainWindow>();

        appHost.Start();
        app.Run(mainWindow);

        appHost.StopAsync().GetAwaiter().GetResult();
    }
}
