using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;
using Victoria;
using Discord;
using System.Collections.Concurrent;
using Victoria.EventArgs;
using Victoria.Enums;

namespace HolyHomie.Services
{
    public class MusicHandler
    {
        private readonly DiscordSocketClient _client;
        public static CommandService? _cmdService;
        private readonly IServiceProvider _services;
        private readonly LavaNode _lavaNode;
        private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _disconnectTokens;

        public MusicHandler(DiscordSocketClient client, CommandService cmdService, IServiceProvider services, LavaNode lavaNode)
        {
            _client = client;
            _cmdService = cmdService;
            _services = services;
            _lavaNode = lavaNode;
            _disconnectTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();
        }

        public async Task InitializeAsync()
        {
            await _cmdService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            _cmdService.Log += LogAsync;
            _client.MessageReceived += HandleMessageAsync;
            _client.Ready += ReadyAsync;
            _lavaNode.OnTrackStarted += TrackStartedAsync;
            _lavaNode.OnTrackEnded += TrackEndedAsync;
        }

        private async Task HandleMessageAsync(SocketMessage socketMessage)
        {
            var argPos = 0;

            var message = socketMessage as SocketUserMessage;
            if (message is null)
                return;

            if (message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.HasCharPrefix('!', ref argPos) || !message.Author.IsBot || socketMessage.Channel.Id == 810298193945428039)
            {
                    var context = new SocketCommandContext(_client, message);
                    var result = await _cmdService.ExecuteAsync(context, argPos, _services);
            }
        }

        private async Task ReadyAsync()
        {
            if (!_lavaNode.IsConnected)
            {
                await _lavaNode.ConnectAsync();
            }
        }

        private async Task TrackStartedAsync(TrackStartEventArgs arg)
        {
            var builder = new EmbedBuilder()
                                .WithDescription($"Сейчас играет: {arg.Track.Title}")
                                .WithColor(new Color(69, 230, 255))
                                .WithThumbnailUrl(await arg.Track.FetchArtworkAsync());
                                var embed = builder.Build();
            await arg.Player.TextChannel.SendMessageAsync(embed: embed);
            if (!_disconnectTokens.TryGetValue(arg.Player.VoiceChannel.Id, out var value))
            {
                return;
            }

            if (value.IsCancellationRequested)
            {
                return;
            }

            value.Cancel(true);
        }
        private async Task TrackEndedAsync(TrackEndedEventArgs args)
        {
            if (args.Reason != TrackEndReason.Finished)
            {
                return;
            }

            var player = args.Player;
            if (!player.Queue.TryDequeue(out var lavaTrack))
            {
                var builder = new EmbedBuilder()
                    .WithDescription($"Очередь закончилась, закажи что нить, чтобы продолжить веселье!")
                    .WithColor(new Color(69, 230, 255));
                var embed = builder.Build();
                await player.TextChannel.SendMessageAsync(embed: embed);
                _ = InitiateDisconnectAsync(args.Player, TimeSpan.FromMinutes(5));
                return;
            }

            if (lavaTrack is null)
            {
                var builder = new EmbedBuilder()
                    .WithDescription($"Дальше идёт не песня!")
                    .WithColor(new Color(69, 230, 255));
                var embed = builder.Build();
                await player.TextChannel.SendMessageAsync(embed: embed);
                return;
            }

            await args.Player.PlayAsync(lavaTrack);
        }
        private async Task InitiateDisconnectAsync(LavaPlayer player, TimeSpan timeSpan)
        {
            if (!_disconnectTokens.TryGetValue(player.VoiceChannel.Id, out var value))
            {
                value = new CancellationTokenSource();
                _disconnectTokens.TryAdd(player.VoiceChannel.Id, value);
            }
            else if (value.IsCancellationRequested)
            {
                _disconnectTokens.TryUpdate(player.VoiceChannel.Id, new CancellationTokenSource(), value);
                value = _disconnectTokens[player.VoiceChannel.Id];
            }

            var isCancelled = SpinWait.SpinUntil(() => value.IsCancellationRequested, timeSpan);
            if (isCancelled)
            {
                return;
            }

            await _lavaNode.LeaveAsync(player.VoiceChannel);
        }

        private Task LogAsync(LogMessage logMessage)
        {
            Console.WriteLine(logMessage.Message);
            return Task.CompletedTask;
        }
    }
}
