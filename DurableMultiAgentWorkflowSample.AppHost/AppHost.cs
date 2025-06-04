var builder = DistributedApplication.CreateBuilder(args);

var aoaiEndpoint = builder.AddParameter("aoai-endpoint");
var aoaiModelDeploymentName = builder.AddParameter("aoai-modeldeploymentname");

builder.AddAzureFunctionsProject<Projects.DurableMultiAgentWorkflowSample_Workflow>("durablemultiagentworkflowsample-workflow")
    .WithEnvironment("AOAI__Endpoint", aoaiEndpoint)
    .WithEnvironment("AOAI__ModelDeploymentName", aoaiModelDeploymentName);

builder.Build().Run();
