using backend.main.configurations.resource.elasticsearch;
using backend.main.services.implementation;
using backend.main.services.interfaces;
using backend.main.utilities.implementation;
using backend.main.utilities.interfaces;

using event_indexer;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Logger.Configure(options =>
{
    options.EnableFileLogging = true;
    options.MinFileLevel = backend.main.utilities.interfaces.LogLevel.Warn;
    options.LogDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
});

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddSingleton(Logger.GetOptions());
builder.Services.AddSingleton<ICustomLogger, FileLogger>();
builder.Services.AddAppElasticsearch(builder.Configuration);
builder.Services.AddSingleton<ElasticsearchCircuitBreaker>();
builder.Services.AddSingleton<IEventSearchService, EventSearchService>();
builder.Services.Configure<EventIndexerOptions>(options =>
{
    options.BootstrapServers =
        builder.Configuration["KAFKA_BOOTSTRAP_SERVERS"] ?? EventIndexerOptions.DefaultBootstrapServers;
    options.Topic =
        builder.Configuration["KAFKA_TOPIC"] ?? EventIndexerOptions.DefaultTopic;
    options.GroupId =
        builder.Configuration["KAFKA_GROUP_ID"] ?? EventIndexerOptions.DefaultGroupId;
    options.DlqTopic =
        builder.Configuration["KAFKA_DLQ_TOPIC"] ?? EventIndexerOptions.DefaultDlqTopic;
});
builder.Services.AddHostedService<EventIndexingWorker>();

var host = builder.Build();
Logger.SetInstance(host.Services.GetRequiredService<ICustomLogger>());
await host.RunAsync();
