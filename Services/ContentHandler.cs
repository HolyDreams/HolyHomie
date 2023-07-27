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
        SocketRole roleDurak;
        SocketRole roleClown;
        SocketRole roleCinema;
        SocketRole roleMale;
        SocketRole roleWoman;
        SocketRole roleTrans;
        SocketRole roleHelicopter;
        SocketRole roleAmong;

        SocketTextChannel channelRole;

        Emote emoteRoleAnyGame;
        Emote emoteRoleWc3;
        Emote emoteRoleHS;
        Emote emoteRoleSiGame;
        Emote emoteRoleTabletop;
        Emote emoteRoleCSGO;
        Emote emoteRoleDota;
        Emote emoteRoleDurak;
        Emote emoteRoleCinema;
        Emote emoteRoleAmong;
        //Emote emoteRoleClown;




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
            roleDurak = _main.GetRole(1013818426235555881);
            roleClown = _main.GetRole(1012073198566785124);
            roleCinema = _main.GetRole(912408613446893619);
            roleMale = _main.GetRole(837369024488013884);
            roleWoman = _main.GetRole(837368897690009640);
            roleTrans = _main.GetRole(837369331670581308);
            roleHelicopter = _main.GetRole(837371435906629662);
            roleAmong = _main.GetRole(1015039700362350604);

            channelRole = _main.GetTextChannel(837369539594551386);

            emoteRoleAnyGame = Emote.Parse("<:roleAnyGame:1036055850340597760>");
            emoteRoleAmong = Emote.Parse("<:roleAmongUS:1036055843466137660>");
            emoteRoleCinema = Emote.Parse("<:roleCinema:1036055841981341806>");
            emoteRoleWc3 = Emote.Parse("<:roleWC3:1036055835979288667>");
            emoteRoleHS = Emote.Parse("<:roleHS:1036055838114201650>");
            emoteRoleSiGame = Emote.Parse("<:roleSIGame:1036055840433639534>");
            emoteRoleTabletop = Emote.Parse("<:roleTabletop:1036055848822251561>");
            emoteRoleCSGO = Emote.Parse("<:roleCSGO:1036055844833460324>");
            emoteRoleDota = Emote.Parse("<:roleDota:1036055851871510599>");
            emoteRoleDurak = Emote.Parse("<:Nasrano:1129503440301731972>");
            //emoteRoleClown = Emote.Parse("<:fritClown:978002471886663740>")

            var messages = await channelRole.GetMessagesAsync(1).FlattenAsync();
            rolesMessage = messages.First().Id;

            //var msg = await channelRole.GetMessageAsync(1030525327677599785);

            //При необходимости добавтиь какой то смайл к сообщению с ролями, дописать ниже.
            //await messages.First().AddReactionAsync(emoteRoleAnyGame);
            //await messages.First().AddReactionAsync(emoteRoleWc3);
            //await messages.First().AddReactionAsync(emoteRoleHS);
            //await messages.First().AddReactionAsync(emoteRoleSiGame);
            //await messages.First().AddReactionAsync(emoteRoleTabletop);
            //await messages.First().AddReactionAsync(emoteRoleDurak);
            //await messages.First().AddReactionAsync(emoteRoleCSGO);
            //await messages.First().AddReactionAsync(emoteRoleDota);
            //await messages.First().AddReactionAsync(emoteRoleAmong);
            //await messages.First().AddReactionAsync(new Emoji("🤡"));
            //await messages.First().AddReactionAsync(emoteRoleCinema);
            //await messages.First().AddReactionAsync(new Emoji("🚹"));
            //await messages.First().AddReactionAsync(new Emoji("🚺"));
            //await messages.First().AddReactionAsync(new Emoji("⚧"));
            //await messages.First().AddReactionAsync(new Emoji("🚁"));
        }
        public async Task MessageRecivedAsync(SocketMessage socketMessage)
        {
            var message = socketMessage as SocketUserMessage;
            SocketGuild guild = ((SocketGuildChannel)message.Channel).Guild;
            var emoteMasterCat = Emote.Parse("<:MasterCAT:822388560790814781>");
            var emoteHSHi = Emote.Parse("<:hshi:706589400401707018>");
            var emoteVibeHi = Emote.Parse("<:vibeHi:1031870617428963368>");
            var emoteNikifor = Emote.Parse("<:Nikifor:1031674334277816370>");
            var emoteDurak = Emote.Parse("<:Nasrano:1129503440301731972>");
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

            if (content.Split(" ").Any(a => a.Contains(":MasterCAT:")))
                await message.AddReactionAsync(emoteMasterCat);
            if (content.Split(" ").Any(a => a.Contains(":hshi:")))
                await message.AddReactionAsync(emoteHSHi);
            if (filtersVibeHi.Any(q => content.Split(" ").Any(a => a.Contains(q))))
                await message.AddReactionAsync(emoteVibeHi);
            if (content.Split(" ").Any(a => a.Contains(":Nasrano:")))
                await message.AddReactionAsync(emoteDurak);
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
        }
        public async Task ReactionAddAsync(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            var contextMessage = await message.GetOrDownloadAsync();
            if (reaction.UserId == _client.CurrentUser.Id) return;
            if (message.Id == rolesMessage)
            {
                SocketRole role = null;
                if (reaction.Emote.Name == emoteRoleAnyGame.Name)
                    role = roleAnyGame;
                else if (reaction.Emote.Name == emoteRoleWc3.Name)
                    role = roleWc3;
                else if (reaction.Emote.Name == emoteRoleHS.Name)
                    role = roleHS;
                else if (reaction.Emote.Name == emoteRoleSiGame.Name)
                    role = roleSiGame;
                else if (reaction.Emote.Name == emoteRoleTabletop.Name)
                    role = roleTabletop;
                else if (reaction.Emote.Name == emoteRoleCSGO.Name)
                    role = roleCSGO;
                else if (reaction.Emote.Name == emoteRoleDota.Name)
                    role = roleDota;
                else if (reaction.Emote.Name == emoteRoleDurak.Name)
                    role = roleDurak;
                else if (reaction.Emote.Name == "\U0001f921")
                    role = roleClown;
                else if (reaction.Emote.Name == emoteRoleCinema.Name)
                    role = roleCinema;
                else if (reaction.Emote.Name == "🚹")
                    role = roleMale;
                else if (reaction.Emote.Name == "🚺")
                    role = roleWoman;
                else if (reaction.Emote.Name == "⚧")
                    role = roleTrans;
                else if (reaction.Emote.Name == "🚁")
                    role = roleHelicopter;
                else if (reaction.Emote.Name == emoteRoleAmong.Name)
                    role = roleAmong;

                await contextMessage.RemoveReactionAsync(reaction.Emote, reaction.UserId);
                if (!_main.GetUser(reaction.UserId).Roles.Contains(role))
                    await _main.GetUser(reaction.UserId).AddRoleAsync(role);
                else
                    await _main.GetUser(reaction.UserId).RemoveRoleAsync(role);
            }
        }

        private async Task NewUserJoin(SocketGuildUser socketGuildUser)
        {
            await socketGuildUser.AddRoleAsync(678975609447120927);
        }
    }
}
