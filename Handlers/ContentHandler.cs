using Discord.Commands;
using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using static System.Collections.Specialized.BitVector32;
using System.Data;
using HolyHomie.Services;
using HolyHomie.Models;
using YamlDotNet.Core.Tokens;

namespace HolyHomie.Handlers
{
    public class ContentHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly LavalinkAudio _audio;

        ulong serverID = 639467461732859914;
        SocketGuild _main;
        SocketTextChannel? _musicChannel;
        List<MessagesAwaitResults> messagesAwaitResult = new List<MessagesAwaitResults>();

        Emoji one = new Emoji(":one:");
        Emoji two = new Emoji(":two:");
        Emoji three = new Emoji(":three:");
        Emoji four = new Emoji(":four:");
        Emoji five = new Emoji(":five:");
        Emoji six = new Emoji(":six:");
        Emoji seven = new Emoji(":seven:");
        Emoji eight = new Emoji(":eight:");
        Emoji nine = new Emoji(":nine:");
        Emoji ten = new Emoji(":keycap_ten:");
        Emoji cancel = new Emoji(":x:");

        Emoji[] numbers = new Emoji[10];

        public ContentHandler(DiscordSocketClient client, LavalinkAudio audio)
        {
            _client = client;
            _audio = audio;
        }

        public async Task InitializeAsync()
        {
            try
            {
                _client.Ready += OnReady;
                _client.MessageReceived += MessageRecivedAsync;
                _client.ReactionAdded += ReactionAddAsync;
                _client.UserJoined += NewUserJoin;
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
            }

        }
        private Task OnReady()
        {
            _main = _client.GetGuild(serverID);

            if (_main != null)
                _musicChannel = _main.GetTextChannel(1115321643175260181);

            numbers = new Emoji[10] { one, two, three, four, five, six, seven, eight, nine, ten };

            return Task.CompletedTask;
        }
        public async Task MessageRecivedAsync(SocketMessage socketMessage)
        {
            var message = socketMessage as SocketUserMessage;

            if (message.Author.IsBot)
                return;

            var emoteNikifor = Emote.Parse("<:Nikifor:1031674334277816370>");
            var p = new Emoji("🇵");
            var i = new Emoji("🇮");
            var z = new Emoji("🇿");
            var d = new Emoji("🇩");
            var a = new Emoji("🇦");
            var b = new Emoji("🇧");
            var l = new Emoji("🇱");
            var y = new Emoji("🇾");
            var t = new Emoji("🇹");
            string content = message.Content;
            string[] filtersVibeHi = { ":vibeHi:", ":mirake1Dance:" };

            if (message.Author.Id != 284953111603314688 || message.Author.Id != 1030169288482504796)
            {
                var messageContent = message.Content.ToLower().Trim('!', '.', ',', '?');
                if (messageContent.Trim(' ').Equals("да") || messageContent.Trim(' ').Equals("da") || messageContent.EndsWith(" да") || messageContent.EndsWith(" da"))
                {
                    await message.AddReactionAsync(p);
                    await message.AddReactionAsync(i);
                    await message.AddReactionAsync(z);
                    await message.AddReactionAsync(d);
                    await message.AddReactionAsync(a);
                }
                if (content.Split(" ").Any(a => a.Contains(":Nikifor:")))
                {
                    await message.AddReactionAsync(b);
                    await message.AddReactionAsync(l);
                    await message.AddReactionAsync(y);
                    await message.AddReactionAsync(a);
                    await message.AddReactionAsync(t);
                }
            }
            if (message.Author.Id == 660180184238129157)
            {
                await message.AddReactionAsync(emoteNikifor);
            }
            if (message.Channel == _musicChannel)
            {
                var playStarts = new string[] { "!p ", "!play ", "!р " };
                if (playStarts.Any(a => content.ToLower().StartsWith(a)))
                {
                    if (content.Split(' ').Length > 0)
                    {
                        var searchResult = await _audio.SearchAsync(content.Substring(content.IndexOf(' ') + 1));
                        if (searchResult == null || searchResult.Count == 0)
                        {
                            await message.ReplyAsync("Ничего не найдено по данному запросу!");
                            return;
                        }

                        if (searchResult.Count == 1)
                        {
                            var author = message.Author;
                            await message.ReplyAsync(await _audio.PlayAsync(author as IVoiceState, author as SocketGuildUser, _main, searchResult[0].Url));
                        }
                    }
                }

                var skipStarts = new string[] { "!s ", "!skip" };
                if (skipStarts.Any(a => content.ToLower().StartsWith(a)))
                {
                    await _audio.SkipTrackAsync(_main);
                }

                var leaveStarts = new string[] { "!l ", "!leave" };
                if (leaveStarts.Any(a => content.ToLower().StartsWith(a)))
                {
                    await _audio.LeaveAsync(_main);
                }

                if (content.ToLower().StartsWith("!list"))
                {
                    await message.ReplyAsync(embed: await _audio.ListAsync(_main));
                }

                if (content.ToLower().StartsWith("!now"))
                {
                    await message.ReplyAsync(await _audio.NowPlayingAsync(_main));
                }

            }
        }
        public async Task ReactionAddAsync(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            var contextMessage = await message.GetOrDownloadAsync();

            if (reaction.UserId == _client.CurrentUser.Id)
                return;

            var needMessage = messagesAwaitResult.FirstOrDefault(a => a.ID == message.Id);
            if (needMessage != null)
            {
                messagesAwaitResult.Remove(needMessage);
                string urlTrack = "";
                var emote = reaction.Emote;
                for (int i = 0; i < numbers.Length; i++)
                {
                    if (numbers[i] as IEmote == emote)
                        urlTrack = needMessage.tracks[i].Url;
                }

                message.Value.DeleteAsync();

                if (!string.IsNullOrWhiteSpace(urlTrack))
                {
                    var author = needMessage.User;
                    await needMessage.Message.ReplyAsync(await _audio.PlayAsync(author as IVoiceState, author as SocketGuildUser, _main, urlTrack));
                }
            }

            if (reaction.UserId == _client.CurrentUser.Id)
                return;
        }

        private async Task NewUserJoin(SocketGuildUser socketGuildUser)
        {
            await socketGuildUser.AddRoleAsync(678975609447120927);
        }
    }
}
