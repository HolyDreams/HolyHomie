using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;

namespace HolyHomie.Handlers
{
    public class LogHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;

        SocketGuild _guild;

        public LogHandler(DiscordSocketClient client, CommandService commands, IConfigurationRoot config)
        {
            _client = client;
            _commands = commands;
            _config = config;
        }

        public async Task InitializeAsync()
        {
            _client.Ready += ReadyAsync;
            _client.MessageReceived += MessageRecivedAsync;
            _client.UserBanned += BanAsync;
            _client.UserUnbanned += UnbanAsync;
            _client.ReactionAdded += ReactionAddAsync;
            _client.ReactionRemoved += ReactionRemoveAsync;
            _client.UserJoined += NewUserJoiAsync;
            _client.UserVoiceStateUpdated += VoiceActiveAsync;
        }

        public Task ReadyAsync()
        {
            _guild = _client.GetGuild(547862107149041685);
            return Task.CompletedTask;
        }

        public static async Task WriteLogMessage(string message)
        {
            Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss]") + " " + message);
        }
        public async Task MessageRecivedAsync(SocketMessage message)
        {
            if (message is null) return;
            if (message.Author.IsBot) return;

            var author = message.Author.Username;
            var channel = message.Channel.Name;
            Console.WriteLine($"{DateTime.Now.ToString("[HH:mm:ss]")} {author} написал в {channel}: {message.Content}");
        }

        public async Task BanAsync(SocketUser user, SocketGuild guild)
        {
            Console.WriteLine($"{DateTime.Now.ToString("[HH:mm:ss]")} {user} был забанен на {guild}");
        }
        public async Task UnbanAsync(SocketUser user, SocketGuild guild)
        {
            Console.WriteLine($"{DateTime.Now.ToString("[HH:mm:ss]")} {user} был разбанен на {guild}");
        }
        public async Task ReactionAddAsync(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            var contextMessage = await message.GetOrDownloadAsync();
            if (reaction.UserId == _client.CurrentUser.Id) return;

            var author = _guild.GetUser(reaction.UserId).Username;
            var emoteName = reaction.Emote.Name;

            Console.WriteLine($"{DateTime.Now.ToString("[HH:mm:ss]")} {author} поставил {emoteName}");
        }
        public async Task ReactionRemoveAsync(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            var contextMessage = await message.GetOrDownloadAsync();
            if (reaction.UserId == _client.CurrentUser.Id) return;

            var author = _guild.GetUser(reaction.UserId).Username;
            var emoteName = reaction.Emote.Name;

            Console.WriteLine($"{DateTime.Now.ToString("[HH:mm:ss]")} {author} убрал {emoteName}");
        }
        public async Task NewUserJoiAsync(SocketGuildUser user)
        {
            var author = user.Username;

            Console.WriteLine($"{DateTime.Now.ToString("[HH:mm:ss]")} {author} зашёл на сервер.");
        }
        public async Task VoiceActiveAsync(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            if (user.IsBot) return;
            if (newState.IsStreaming)
                Console.WriteLine($"{DateTime.Now.ToString("[HH:mm:ss]")} {user} включил стрим");
            if (newState.IsMuted)
                Console.WriteLine($"{DateTime.Now.ToString("[HH:mm:ss]")} {user} в мьюте.");
            if (oldState.IsMuted)
                Console.WriteLine($"{DateTime.Now.ToString("[HH:mm:ss]")} {user} убрали мут.");
            if (newState.IsVideoing)
                Console.WriteLine($"{DateTime.Now.ToString("[HH:mm:ss]")} {user} включил камеру.");
            if (oldState.IsVideoing)
                Console.WriteLine($"{DateTime.Now.ToString("[HH:mm:ss]")} {user} выключил камеру.");
            if (oldState.VoiceChannel == newState.VoiceChannel) return;
            if (oldState.VoiceChannel != null)
                Console.WriteLine($"{DateTime.Now.ToString("[HH:mm:ss]")} {user} вышел из {oldState.VoiceChannel.Name}.");
            if (newState.VoiceChannel != null)
                Console.WriteLine($"{DateTime.Now.ToString("[HH:mm:ss]")} {user} зашёл на {newState.VoiceChannel.Name}.");
        }
    }
}
