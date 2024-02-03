using Core.Domain;
using Core.Interfaces;
using DisCatSharp.Hosting.DependencyInjection;
using HolyHomie.Services;
using Logic.Lava;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

public class Program
{
    public static void Main(string[] args)
    {
        MainAsync().GetAwaiter().GetResult();
    }

    public static async Task MainAsync()
    {
        var logger = Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .CreateLogger();

        logger.Information("Запускаю бота...");

        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        var builder = Host.CreateDefaultBuilder();
        builder.ConfigureServices(services =>
        {
            services.AddLogging(a => a.AddSerilog());
            services.AddSingleton<ILavaQueue, LavaQueue>();
            services.Configure<MainOptions>(config.GetSection("MainOptions"));
            services.AddHostedService<RunLavalink>();
            services.AddHostedService<RunDiscord>();
        });

        var app = builder.Build();
        await app.RunAsync();
        await Task.Delay(-1);
    }
}