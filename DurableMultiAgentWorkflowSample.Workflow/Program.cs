using Azure.AI.OpenAI;
using Azure.Identity;
using DurableMultiAgentWorkflowSample.Workflow.Agents;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

var builder = FunctionsApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddAzureOpenAIChatCompletion(
    builder.Configuration["AOAI:ModelDeploymentName"]!,
    builder.Configuration["AOAI:Endpoint"]!,
    new AzureCliCredential());
builder.Services.AddKernel();
builder.Services.AddTransient<AgentDefinitions>();
builder.Services.AddKeyedTransient(
    AgentDefinitions.WriterAgentName,
    (sp, _) => sp.GetRequiredService<AgentDefinitions>().CreateWriterAgent());
builder.Services.AddKeyedTransient(
    AgentDefinitions.ReviewerAgentName, 
    (sp, _) => sp.GetRequiredService<AgentDefinitions>().CreateReviewerAgent());
builder.Services.AddKeyedTransient(
    AgentDefinitions.ApproverAgentName, 
    (sp, _) => sp.GetRequiredService<AgentDefinitions>().CreateApproverAgent());

builder.ConfigureFunctionsWebApplication();
builder.Build().Run();
