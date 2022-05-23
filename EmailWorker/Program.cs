using EmailWorker;
using RemesasAPI;
using RemesasAPI.Entities;

var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false).Build();
var emailConfig = config.GetSection("EmailConfiguration").Get<EmailConfiguration>();

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton(emailConfig);
        services.AddSingleton<ApiContext>();
        services.AddSingleton<MonitorLoop>();
        services.AddHostedService<QueuedHostedService>();
        services.AddSingleton<IBackgroundTaskQueue>(ctx =>
        {
            // if (!int.TryParse(hostContext.Configuration["QueueCapacity"], out var queueCapacity))
            //     queueCapacity = 100;
            return new BackgroundTaskQueue(100);
        });
    })
    .Build();

var monitorLoop = host.Services.GetRequiredService<MonitorLoop>();
monitorLoop.StartMonitorLoop();

await host.RunAsync();
