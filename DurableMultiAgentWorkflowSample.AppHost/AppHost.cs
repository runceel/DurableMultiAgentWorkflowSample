var builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureFunctionsProject<Projects.DurableMultiAgentWorkflowSample_Workflow>("durablemultiagentworkflowsample-workflow");

builder.Build().Run();
