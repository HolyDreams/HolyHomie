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

namespace HolyHomie.Services
{
    public class RunDiscord : IHostedService
    {
        private readonly MainOptions _options;
        private readonly ILogger<RunDiscord> _logger;
        private readonly IServiceProvider _provider;

        public RunDiscord(IOptions<MainOptions> options, ILogger<RunDiscord> logger, IServiceProvider provider)
        {
            _options = options.Value;
            _logger = logger;
            _provider = provider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            StartAllAsync().GetAwaiter().GetResult();

            return Task.CompletedTask;
        }

        private async Task StartAllAsync()
        {
            try
            {
                var client = new DiscordClient(new DiscordConfiguration
                {
                    Token = _options.Discord.Token,
                    TokenType = TokenType.Bot,
                    AutoReconnect = true,
                    MinimumLogLevel = LogLevel.Debug,
                    Intents = DiscordIntents.All,
                    LogTimestampFormat = "dd.MM.yyг - HH:mm:ss"
                });

                client.RegisterEventHandlers(Assembly.LoadFrom("Logic.Events.dll"));
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

                slash.RegisterGlobalCommands(Assembly.LoadFrom("Logic.SlashComands.dll"));

                await client.ConnectAsync();

                _logger.LogInformation("Подключаюсь к lavalink...");

                var lava = client.UseLavalink();
                await lava.ConnectAsync(new LavalinkConfiguration());
                _logger.LogInformation("Успешно подключено к lavalink");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
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
