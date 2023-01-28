using lacuna_genetics;
using lacuna_genetics.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = new ConfigurationBuilder();

builder.SetBasePath(Directory.GetCurrentDirectory())
       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
       .AddEnvironmentVariables();

IConfiguration config = builder.Build();

var appConfig = config.GetSection(nameof(LacunaConfig)).Get<LacunaConfig>();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddTransient<LacunaService>();
        services.AddHttpClient<Client>(cfg =>
        {
            cfg.BaseAddress = new Uri(appConfig?.UriBase!);
        });
    })
    .Build();


var hostService = host.Services.GetRequiredService<LacunaService>();
await hostService.ExecuteAsync(appConfig!);




