using DisCatSharp;
using DisCatSharp.Enums;
using Core.Domain;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using DisCatSharp.Exceptions;
using DisCatSharp.Lavalink;
using System.Reflection;
using DisCatSharp.Interactivity.Extensions;
using DisCatSharp.Interactivity;
using DisCatSharp.Interactivity.Enums;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DisCatSharp.ApplicationCommands;
using Serilog;

namespace HolyHomie.Services
{
    public class RunDiscord : IHostedService
    {
        private readonly Discord _options;
        private readonly IServiceProvider _provider;

        public RunDiscord(IOptions<MainOptions> options, IServiceProvider provider)
        {
            _options = options.Value.Discord;
            _provider = provider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await StartAllAsync();

            return;
        }

        private async Task StartAllAsync()
        {
            Log.Information("Запускаю дискорд бота");
            try
            {
                var client = new DiscordClient(new DiscordConfiguration
                {
                    Token = _options.Token,
                    TokenType = TokenType.Bot,
                    AutoReconnect = true,
                    MinimumLogLevel = LogLevel.Debug,
                    Intents = DiscordIntents.All,
                    LogTimestampFormat = "dd.MM.yyг - HH:mm:ss"
                });

                client.RegisterEventHandlers(Assembly.LoadFrom(Path.Combine(AppContext.BaseDirectory, "Logic.Events.dll")));
                client.ClientErrored += OnClientErrored;
                client.Ready += OnReady;

                client.UseInteractivity(new InteractivityConfiguration
                {
                    Timeout = TimeSpan.FromMinutes(2),
                    AckPaginationButtons = true,
                    PaginationBehaviour = PaginationBehaviour.Ignore
                });

                var slash = client.UseApplicationCommands(new ApplicationCommandsConfiguration
                {
                    ServiceProvider = _provider,
                    EnableDefaultHelp = false
                });

                slash.RegisterGlobalCommands(Assembly.LoadFrom(Path.Combine(AppContext.BaseDirectory, "Logic.SlashComands.dll")));

                await client.ConnectAsync();

                await Task.Delay(30000);

                Log.Information("Подключаюсь к lavalink...");

                var lava = client.UseLavalink();
                await lava.ConnectAsync(new LavalinkConfiguration());
                Log.Information("Успешно подключено к lavalink");
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }
        private Task OnReady(DiscordClient sender, ReadyEventArgs e)
        {
            sender.UpdateStatusAsync(new DiscordActivity("https://www.twitch.tv/freemasom", ActivityType.Watching)).GetAwaiter().GetResult();

            return Task.CompletedTask;
        }

        private Task OnClientErrored(DiscordClient sender, ClientErrorEventArgs e)
        {
            if (e.Exception is NotFoundException)
            {
                e.Handled = true;
                return Task.CompletedTask;
            }

            if (e.Exception is BadRequestException)
            {
                e.Handled = true;
                return Task.CompletedTask;
            }

            sender.Logger.LogError($"Exception occured: {e.Exception.GetType()}: {e.Exception.Message}");
            sender.Logger.LogError($"Stacktrace: {e.Exception.GetType()}: {e.Exception.StackTrace}");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
