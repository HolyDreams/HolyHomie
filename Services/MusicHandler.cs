using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;
using Victoria;
using Discord;
using System.Collections.Concurrent;
using Victoria.EventArgs;

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
            if (!_disconnectTokens.TryGetValue(arg.Player.VoiceChannel.Id, out var value))
            {
                return;
            }

            if (value.IsCancellationRequested)
            {
                return;
            }

            value.Cancel(true);
            Console.WriteLine("Автодисконект отключён.");
            await arg.Player.TextChannel.SendMessageAsync("Автодисконект отключён.");
        }

        private async Task TrackEndedAsync(TrackEndedEventArgs args)
        {
            if (args.Reason != Victoria.Enums.TrackEndReason.Finished)
            {
                return;
            }

            var player = args.Player;
            if (!player.Queue.TryDequeue(out var lavaTrack))
            {
                await player.TextChannel.SendMessageAsync("Очередь пуста! Добавь музычки для удовольствия");
                //_ = InitiateDisconnectAsync(args.Player, TimeSpan.FromMinutes(5d));
                //Console.WriteLine("Автодисконект включён.");
                return;
            }

            if (!(lavaTrack is LavaTrack track))
            {
                await player.TextChannel.SendMessageAsync("Следующая композиция не песня!");
                return;
            }

            await args.Player.PlayAsync(track);
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

            Console.WriteLine($"Автодисконект был установлен! Отключение через {timeSpan}...");
            var isCancelled = SpinWait.SpinUntil(() => value.IsCancellationRequested, timeSpan);

            if (isCancelled) return;

            await _lavaNode.LeaveAsync(player.VoiceChannel);
            Console.WriteLine($"Автодиcконект прошёл успешно!");
        }

        private Task LogAsync(LogMessage logMessage)
        {
            Console.WriteLine(logMessage.Message);
            return Task.CompletedTask;
        }
    }
}
