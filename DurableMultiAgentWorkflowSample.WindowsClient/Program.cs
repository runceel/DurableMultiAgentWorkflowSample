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

        var appHost = builder.Build();
        var app = appHost.Services.GetRequiredService<App>();
        var mainWindow = appHost.Services.GetRequiredService<MainWindow>();

        appHost.Start();
        app.Run(mainWindow);

        appHost.StopAsync().GetAwaiter().GetResult();
    }
}
