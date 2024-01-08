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
using HolyHomie.Handlers;
using Victoria.Node;
using Victoria.Player;
using HolyHomie.Modules;

namespace HolyHomie
{
    public class Program
    {
        public static CancellationTokenSource cts = new CancellationTokenSource();
        private DiscordSocketClient _client;
        private LavaNode _lavaNode;

        static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult();
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        }

        private static void CurrentDomain_ProcessExit(object? sender, EventArgs e)
        {
            cts.Cancel();
            Thread.Sleep(50);
            cts.Dispose();

            var processList = Process.GetProcessesByName("java");
            if (processList.Length > 0)
                processList[0].Kill();
        }

        public async Task MainAsync()
        {
            await StartLavalinkAsync();

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
                    LogLevel = LogSeverity.Info,
                    UseInteractionSnowflakeDate = false

                }))
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<InteractionHandler>()
                .AddSingleton<LogHandler>()
                .AddLavaNode(x => x.SelfDeaf = false)
                .AddSingleton<LavalinkAudio>()
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
            _lavaNode = provider.GetRequiredService<LavaNode>();

            var contentCommands = provider.GetRequiredService<ContentHandler>();
            await contentCommands.InitializeAsync();

            var logHandler = provider.GetRequiredService<LogHandler>();
            await logHandler.InitializeAsync();

            await _client.LoginAsync(TokenType.Bot, config["tokens:discord"]);
            await _client.StartAsync();
            await _client.SetGameAsync("TTT с Лёхой");
            _client.Log += LogAsync;

            _client.Ready += async () =>
            {
                if (!_lavaNode.IsConnected)
                {
                    try
                    {
                        await _lavaNode.ConnectAsync();
                    }
                    catch (Exception ex)
                    {
                        await Console.Out.WriteLineAsync( ex.Message);
                    }
                }

                await Console.Out.WriteLineAsync("Подключенно!");

                try
                {
                    await commands.RegisterCommandsGloballyAsync(true);
                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync( ex.Message);
                }
            };

            await Task.Delay(-1);
        }
        public static Task LogAsync(LogMessage logMessage)
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

        static Task StartLavalinkAsync()
        {
            // written knowing full well there might be other java processes running, but I'm developing this for my Raspi
            Console.WriteLine("Запускаю Lavalink");
            try
            {
                var processList = Process.GetProcessesByName("java");
                if (processList.Length == 0)
                {
                    string lavalinkFile = Path.Combine(AppContext.BaseDirectory, "Lavalink", "Lavalink.jar");
                    if (!File.Exists(lavalinkFile)) return Task.CompletedTask;

                    var process = new ProcessStartInfo
                    {
                        FileName = "java",
                        Arguments = $"-jar \"{Path.Combine(AppContext.BaseDirectory, "Lavalink")}/Lavalink.jar\"",
                        WorkingDirectory = Path.Combine(AppContext.BaseDirectory, "Lavalink"),
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    Process.Start(process);
                    Console.WriteLine("Lavalink запущен!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Task.CompletedTask;
        }
    }
}
