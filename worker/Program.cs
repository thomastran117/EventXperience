using backend.Config;

using worker.Tasks;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddApplicationServices();
builder.Services.AddHostedService<EmailWorker>();

var host = builder.Build();
host.Run();
