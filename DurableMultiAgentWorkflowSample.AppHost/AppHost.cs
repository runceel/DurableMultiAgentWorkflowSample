using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var aoaiEndpoint = builder.AddParameter("aoai-endpoint");
var aoaiModelDeploymentName = builder.AddParameter("aoai-modeldeploymentname");

IResourceBuilder<ContainerResource>? durableTaskScheduler = null;
IResourceBuilder<ParameterResource>? durableTaskSchedulerConnectionString = null;
if (builder.ExecutionContext.IsRunMode)
{
    durableTaskScheduler = builder.AddContainer("durable-task-scheduler",
        "mcr.microsoft.com/dts/dts-emulator",
        "latest")
        .WithLifetime(ContainerLifetime.Persistent)
        .WithEndpoint(8080, 8080)
        .WithEndpoint(8082, 8082, scheme: "http", name: "Dashboard");
    durableTaskSchedulerConnectionString = builder.AddParameter("durable-task-scheduler-connectionstring",
        "Endpoint=http://localhost:8080;Authentication=None");
}

var workflow = builder.AddAzureFunctionsProject<Projects.DurableMultiAgentWorkflowSample_Workflow>("workflow")
    .WithEnvironment("AOAI__Endpoint", aoaiEndpoint)
    .WithEnvironment("AOAI__ModelDeploymentName", aoaiModelDeploymentName)
    .WithExternalHttpEndpoints();

if (durableTaskScheduler != null && durableTaskSchedulerConnectionString != null)
{
    workflow.WithEnvironment("DURABLE_TASK_SCHEDULER_CONNECTION_STRING", durableTaskSchedulerConnectionString)
        .WithEnvironment("TASKHUB_NAME", "default")
        .WaitFor(durableTaskScheduler);
}

if (OperatingSystem.IsWindows())
{
    builder.AddProject<Projects.DurableMultiAgentWorkflowSample_WindowsClient>("windows-client")
        .WithReference(workflow)
        .WaitFor(workflow)
        .WithExplicitStart()
        .ExcludeFromManifest();
}

builder.Build().Run();
