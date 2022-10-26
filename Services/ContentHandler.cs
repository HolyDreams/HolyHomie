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

namespace HolyHomie.Services
{
    public class ContentHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;

        ulong serverID = 270119143636860928;
        ulong rolesMessage = 1;
        SocketGuild _main;
        SocketRole roleAnyGame;
        SocketRole roleWc3;
        SocketRole roleHS;
        SocketRole roleSiGame;
        SocketRole roleTabletop;
        SocketRole roleCSGO;
        SocketRole roleDota;
        SocketRole roleWoT;
        SocketRole roleClown;
        SocketRole roleCinema;
        SocketRole roleMale;
        SocketRole roleWoman;
        SocketRole roleTrans;
        SocketRole roleHelicopter;
        SocketRole roleAmong;
        SocketTextChannel channelRole;



        public ContentHandler(DiscordSocketClient client, CommandService commands, IConfigurationRoot config)
        {
            _client = client;
            _commands = commands;
            _config = config;
        }

        public async Task InitializeAsync()
        {
            _client.Ready += OnReady;
            _client.MessageReceived += MessageRecivedAsync;
            _client.ReactionAdded += ReactionAddAsync;
            _client.ReactionRemoved += ReactionRemove;
            _client.UserJoined += NewUserJoin;
        }
        private async Task OnReady()
        {
            _main = _client.GetGuild(serverID);

            roleAnyGame = _main.GetRole(837224947415252992);
            roleWc3 = _main.GetRole(1011968228366504028);
            roleHS = _main.GetRole(837224312577458186);
            roleSiGame = _main.GetRole(739730784356794409);
            roleTabletop = _main.GetRole(837224578870280233);
            roleCSGO = _main.GetRole(933775508380983326);
            roleDota = _main.GetRole(1013818161059078274);
            roleWoT = _main.GetRole(1013818426235555881);
            roleClown = _main.GetRole(1012073198566785124);
            roleCinema = _main.GetRole(912408613446893619);
            roleMale = _main.GetRole(837369024488013884);
            roleWoman = _main.GetRole(837368897690009640);
            roleTrans = _main.GetRole(837369331670581308);
            roleHelicopter = _main.GetRole(837371435906629662);
            roleAmong = _main.GetRole(1015039700362350604);

            channelRole = _main.GetTextChannel(837369539594551386);

            var messages = await channelRole.GetMessagesAsync(1).FlattenAsync();
            rolesMessage = messages.First().Id;

            var msg = await channelRole.GetMessageAsync(1030525327677599785);

            if (msg is IUserMessage message)
            {
                Console.WriteLine("here2");
            }
            //При необходимости добавтиь какой то смайл к сообщению с ролями, дописать ниже.
            //await messages.First().AddReactionAsync(new Emoji("🟪"));
            //await messages.First().AddReactionAsync(new Emoji("🟥"));
            //await messages.First().AddReactionAsync(new Emoji("🟧"));
            //await messages.First().AddReactionAsync(new Emoji("🟦"));
            //await messages.First().AddReactionAsync(new Emoji("🟫"));
            //await messages.First().AddReactionAsync(new Emoji("🟡"));
            //await messages.First().AddReactionAsync(new Emoji("🔴"));
            //await messages.First().AddReactionAsync(new Emoji("🟩"));
            //await messages.First().AddReactionAsync(new Emoji("🤡"));
            //await messages.First().AddReactionAsync(new Emoji("🎦"));
            //await messages.First().AddReactionAsync(new Emoji("🚹"));
            //await messages.First().AddReactionAsync(new Emoji("🚺"));
            //await messages.First().AddReactionAsync(new Emoji("⚧"));
            //await messages.First().AddReactionAsync(new Emoji("🚁"));
            //await messages.First().AddReactionAsync(new Emoji("🟠"));
        }
        public async Task MessageRecivedAsync(SocketMessage socketMessage)
        {
            var message = socketMessage as SocketUserMessage;
            SocketGuild guild = ((SocketGuildChannel)message.Channel).Guild;
            var emoteMasterCat = Emote.Parse("<:MasterCAT:822388560790814781>");
            var emoteHSHi = Emote.Parse("<:hshi:706589400401707018>");
            var emoteVibeHi = Emote.Parse("<:vibeHi:1031870617428963368>");
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
            string[] filtersEmoteMasterCat = { ":MasterCAT:" };
            bool containsEmoteMasterCat = filtersEmoteMasterCat.Any(x => content.Split(" ").Any(y => y.Contains(x)));
            string[] filtersEmoteHSHi = { ":hshi:" };
            bool containsEmoteHSHi = filtersEmoteHSHi.Any(x => content.Split(" ").Any(y => y.Contains(x)));
            string[] filtersVibeHi = { ":vibeHi:" };
            bool containsEmoteVibeHi = filtersVibeHi.Any(x => content.Split(" ").Any(y => y.Contains(x)));
            string[] filtersMirakeDance = { ":mirake1Dance:" };
            bool containsEmoteMirakeDance = filtersMirakeDance.Any(x => content.Split(" ").Any(y => y.Contains(x)));
            string[] filtersNikifor = { ":Nikifor:" };
            bool containsEmoteNikifor = filtersNikifor.Any(x => content.Split(" ").Any(y => y.Contains(x)));

            if (containsEmoteMasterCat)
            {
                await message.AddReactionAsync(emoteMasterCat);
            }
            if (containsEmoteHSHi)
            {
                await message.AddReactionAsync(emoteHSHi);
            }
            if (containsEmoteVibeHi || containsEmoteMirakeDance)
            {
                await message.AddReactionAsync(emoteVibeHi);
            }
            if (message.Author.Id != 284953111603314688 || message.Author.Id != 1030169288482504796)
            {
                if (message.Content.Equals("Да") || message.Content.Equals("Да!") || message.Content.Equals("да") || message.Content.Equals("Да?") || message.Content.Equals("да!") || message.Content.Equals("да?") || message.Content.Equals("ДА") || message.Content.Equals("ДА?") || message.Content.Equals("ДА!") ||
                    message.Content.EndsWith(" Да") || message.Content.EndsWith(" Да!") || message.Content.EndsWith(" Да?") || message.Content.EndsWith(" да!") || message.Content.EndsWith(" да") || message.Content.EndsWith(" да?") || message.Content.EndsWith(" ДА") || message.Content.EndsWith(" ДА?") || message.Content.EndsWith(" ДА!") ||
                    message.Content.Equals("Da") || message.Content.Equals("Da!") || message.Content.Equals("da") || message.Content.Equals("Da?") || message.Content.Equals("da!") || message.Content.Equals("da?") || message.Content.Equals("DA") || message.Content.Equals("DA?") || message.Content.Equals("DA!") ||
                    message.Content.EndsWith(" Da") || message.Content.EndsWith(" Da!") || message.Content.EndsWith(" Da?") || message.Content.EndsWith(" da!") || message.Content.EndsWith(" da") || message.Content.EndsWith(" da?") || message.Content.EndsWith(" DA") || message.Content.EndsWith(" DA?") || message.Content.EndsWith(" DA!"))
                {
                    await message.AddReactionAsync(p);
                    await message.AddReactionAsync(i);
                    await message.AddReactionAsync(z);
                    await message.AddReactionAsync(d);
                    await message.AddReactionAsync(a);
                }
                if (containsEmoteNikifor)
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
        }
        public async Task ReactionAddAsync(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            if (reaction.UserId == _client.CurrentUser.Id) return;
            if (message.Id == rolesMessage)
            {
                if (reaction.Emote.Name == "\U0001f7ea") { await _main.GetUser(reaction.UserId).AddRoleAsync(roleAnyGame); }
                if (reaction.Emote.Name == "\U0001f7e5") { await _main.GetUser(reaction.UserId).AddRoleAsync(roleWc3); }
                if (reaction.Emote.Name == "\U0001f7e7") { await _main.GetUser(reaction.UserId).AddRoleAsync(roleHS); }
                if (reaction.Emote.Name == "\U0001f7e6") { await _main.GetUser(reaction.UserId).AddRoleAsync(roleSiGame); }
                if (reaction.Emote.Name == "\U0001f7eb") { await _main.GetUser(reaction.UserId).AddRoleAsync(roleTabletop); }
                if (reaction.Emote.Name == "\U0001f7e1") { await _main.GetUser(reaction.UserId).AddRoleAsync(roleCSGO); }
                if (reaction.Emote.Name == "🔴") { await _main.GetUser(reaction.UserId).AddRoleAsync(roleDota); }
                if (reaction.Emote.Name == "\U0001f7e9") { await _main.GetUser(reaction.UserId).AddRoleAsync(roleWoT); }
                if (reaction.Emote.Name == "\U0001f921") { await _main.GetUser(reaction.UserId).AddRoleAsync(roleClown); }
                if (reaction.Emote.Name == "🎦") { await _main.GetUser(reaction.UserId).AddRoleAsync(roleCinema); }
                if (reaction.Emote.Name == "🚹") { await _main.GetUser(reaction.UserId).AddRoleAsync(roleMale); }
                if (reaction.Emote.Name == "🚺") { await _main.GetUser(reaction.UserId).AddRoleAsync(roleWoman); }
                if (reaction.Emote.Name == "⚧") { await _main.GetUser(reaction.UserId).AddRoleAsync(roleTrans); }
                if (reaction.Emote.Name == "🚁") { await _main.GetUser(reaction.UserId).AddRoleAsync(roleHelicopter); }
                if (reaction.Emote.Name == "\U0001f7e0") { await _main.GetUser(reaction.UserId).AddRoleAsync(roleAmong); }
            }
        }

        public async Task ReactionRemove(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            if (reaction.UserId == _client.CurrentUser.Id) return;
            if (message.Id == rolesMessage)
            {
                if (reaction.Emote.Name == "\U0001f7ea") { await _main.GetUser(reaction.UserId).RemoveRoleAsync(roleAnyGame); }
                if (reaction.Emote.Name == "\U0001f7e5") { await _main.GetUser(reaction.UserId).RemoveRoleAsync(roleWc3); }
                if (reaction.Emote.Name == "\U0001f7e7") { await _main.GetUser(reaction.UserId).RemoveRoleAsync(roleHS); }
                if (reaction.Emote.Name == "\U0001f7e6") { await _main.GetUser(reaction.UserId).RemoveRoleAsync(roleSiGame); }
                if (reaction.Emote.Name == "\U0001f7eb") { await _main.GetUser(reaction.UserId).RemoveRoleAsync(roleTabletop); }
                if (reaction.Emote.Name == "\U0001f7e1") { await _main.GetUser(reaction.UserId).RemoveRoleAsync(roleCSGO); }
                if (reaction.Emote.Name == "🔴") { await _main.GetUser(reaction.UserId).RemoveRoleAsync(roleDota); }
                if (reaction.Emote.Name == "\U0001f7e9") { await _main.GetUser(reaction.UserId).RemoveRoleAsync(roleWoT); }
                if (reaction.Emote.Name == "\U0001f921") { await _main.GetUser(reaction.UserId).RemoveRoleAsync(roleClown); }
                if (reaction.Emote.Name == "🎦") { await _main.GetUser(reaction.UserId).RemoveRoleAsync(roleCinema); }
                if (reaction.Emote.Name == "🚹") { await _main.GetUser(reaction.UserId).RemoveRoleAsync(roleMale); }
                if (reaction.Emote.Name == "🚺") { await _main.GetUser(reaction.UserId).RemoveRoleAsync(roleWoman); }
                if (reaction.Emote.Name == "⚧") { await _main.GetUser(reaction.UserId).RemoveRoleAsync(roleTrans); }
                if (reaction.Emote.Name == "🚁") { await _main.GetUser(reaction.UserId).RemoveRoleAsync(roleHelicopter); }
                if (reaction.Emote.Name == "\U0001f7e0") { await _main.GetUser(reaction.UserId).RemoveRoleAsync(roleAmong); }
            }
        }

        private async Task NewUserJoin(SocketGuildUser socketGuildUser)
        {
            await socketGuildUser.AddRoleAsync(678975609447120927);
        }
    }
}
