using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration.Yaml;
using Discord.Interactions;
using Discord.Commands;
using HolyHomie.Services;
using Victoria;
using System.Globalization;
using System.Diagnostics;

namespace HolyHomie
{
    public class Program
    {
        public static CancellationTokenSource cts = new CancellationTokenSource();
        private DiscordSocketClient _client;

        static void Main(string[] args)
        {
            var _checkBGRank = new CheckBGRank();
            _checkBGRank.Start();
            new Program().MainAsync().GetAwaiter().GetResult();
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        }

        private static void CurrentDomain_ProcessExit(object? sender, EventArgs e)
        {
            cts.Cancel();
            Thread.Sleep(50);
            cts.Dispose();
        }

        public async Task MainAsync()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddYamlFile("cfg.yml")
                .Build();

            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                services
                .AddSingleton(config)
                .AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig
                {
                    GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.GuildMembers,
                    //LogGatewayIntentWarnings = false,
                    AlwaysDownloadUsers = true,
                    LogLevel = LogSeverity.Info

                }))
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<InteractionHandler>()
                .AddSingleton<LogHandler>()
                .AddSingleton(x => new CommandService(new CommandServiceConfig
                {
                    LogLevel = LogSeverity.Info,
                    DefaultRunMode = Discord.Commands.RunMode.Async
                }))
                .AddSingleton<ContentHandler>()
                )
                .Build();

            await RunAsync(host);
        }

        public async Task RunAsync(IHost host)
        {
            using IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            await provider.GetRequiredService<InteractionHandler>().InitializeAsync();

            var commands = provider.GetRequiredService<InteractionService>();
            _client = provider.GetRequiredService<DiscordSocketClient>();
            var config = provider.GetRequiredService<IConfigurationRoot>();

            var contentCommands = provider.GetRequiredService<ContentHandler>();
            await contentCommands.InitializeAsync();

            var logHandler = provider.GetRequiredService<LogHandler>();
            await logHandler.InitializeAsync();

            _client.Ready += async () =>
            {

                if (IsDebug())
                    await commands.RegisterCommandsToGuildAsync(UInt64.Parse(config["fritGuild"]), true);
                else
                await commands.RegisterCommandsGloballyAsync(true);
            };

            await _client.LoginAsync(TokenType.Bot, config["tokens:discord"]);
            await _client.StartAsync();
            await _client.SetGameAsync("Wacraft 3");
            _client.Log += LogAsync;

            await Task.Delay(-1);
        }
        private Task LogAsync(LogMessage logMessage)
        {
            Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + logMessage.Message);
            return Task.CompletedTask;
        }
        static bool IsDebug()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}
