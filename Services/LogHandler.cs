using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.EventArgs;

namespace HolyHomie.Services
{
    public class LogHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;
        private readonly LavaNode _lavaNode;

        string timeNow = DateTime.Now.ToString("[HH:mm:ss]");

        SocketGuild _guild;

        public LogHandler(DiscordSocketClient client, CommandService commands, IConfigurationRoot config, LavaNode lavaNode)
        {
            _client = client;
            _commands = commands;
            _config = config;
            _lavaNode = lavaNode;
        }

        public async Task InitializeAsync()
        {
            _client.Ready += ReadyAsync;
            _client.MessageReceived += MessageRecivedAsync;
            _client.MessageDeleted += MessageDeleteAsync;
            _client.UserBanned += BanAsync;
            _client.UserUnbanned += UnbanAsync;
            _client.ReactionAdded += ReactionAddAsync;
            _client.ReactionRemoved += ReactionRemoveAsync;
            _client.UserJoined += NewUserJoiAsync;
            _client.UserVoiceStateUpdated += VoiceActiveAsync;
            _lavaNode.OnTrackStarted += TrackStartedAsync;
        }

        public Task ReadyAsync()
        {
            _guild = _client.GetGuild(270119143636860928);
            return Task.CompletedTask;
        }

        public async Task MessageRecivedAsync(SocketMessage message)
        {
            if (message is null) return;
            if (message.Author.IsBot) return;

            var author = message.Author.Username;
            var channel = message.Channel.Name;
            Console.WriteLine($"{timeNow} {author} написал в {channel}: {message.Content}");
        }
        public async Task MessageDeleteAsync(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel)
        {
            var contextMessage = await message.GetOrDownloadAsync();
            var contextChannel = await channel.GetOrDownloadAsync();
            Console.WriteLine($"{timeNow} В канале {contextMessage.Channel.Name} {contextMessage.Author.Username} удалил сообщение.");
        }

        public async Task BanAsync(SocketUser user, SocketGuild guild)
        {
            Console.WriteLine($"{timeNow} {user} был забанен на {guild}");
        }
        public async Task UnbanAsync(SocketUser user, SocketGuild guild)
        {
            Console.WriteLine($"{timeNow} {user} был разбанен на {guild}");
        }
        public async Task ReactionAddAsync(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            var contextMessage = await message.GetOrDownloadAsync();
            if (reaction.UserId == _client.CurrentUser.Id) return;

            var author = _guild.GetUser(reaction.UserId).Username;
            var emoteName = reaction.Emote.Name;

            Console.WriteLine($"{timeNow} {author} поставил {emoteName}");
        }
        public async Task ReactionRemoveAsync(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            var contextMessage = await message.GetOrDownloadAsync();
            if (reaction.UserId == _client.CurrentUser.Id) return;

            var author = _guild.GetUser(reaction.UserId).Username;
            var emoteName = reaction.Emote.Name;

            Console.WriteLine($"{timeNow} {author} убрал {emoteName}");
        }
        public async Task NewUserJoiAsync(SocketGuildUser user)
        {
            var author = user.Username;

            Console.WriteLine($"{timeNow} {author} зашёл на сервер.");
        }
        public async Task TrackStartedAsync(TrackStartEventArgs arg)
        {
            Console.WriteLine($"{timeNow} Сейчас играет {arg.Track.Title}");
        }
        public async Task VoiceActiveAsync(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            if (user.IsBot) return;
            if (newState.IsStreaming)
                Console.WriteLine($"{timeNow} {user} включил стрим");
            if (newState.IsMuted)
                Console.WriteLine($"{timeNow} {user} в мьюте.");
            if (oldState.IsMuted)
                Console.WriteLine($"{timeNow} {user} убрали мут.");
            if (newState.IsVideoing)
                Console.WriteLine($"{timeNow} {user} включил камеру.");
            if (oldState.IsVideoing)
                Console.WriteLine($"{timeNow} {user} выключил камеру.");
            if (oldState.VoiceChannel == newState.VoiceChannel) return;
            if (oldState.VoiceChannel != null)
                Console.WriteLine($"{timeNow} {user} вышел из {oldState.VoiceChannel.Name}.");
            if (newState.VoiceChannel != null)
                Console.WriteLine($"{timeNow} {user} зашёл на {newState.VoiceChannel.Name}.");
            //if (oldState.IsSelfMuted == false)
            //    Console.WriteLine($"{timeNow} {user} выключил микрофон.");
            //if (oldState.IsSelfMuted == false)
            //    Console.WriteLine($"{timeNow} {user} включил микрофон.");
        }
    }
}
