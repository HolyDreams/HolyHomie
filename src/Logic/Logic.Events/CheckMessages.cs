using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;

namespace Logic.Events
{
    [EventHandler]
    public class CheckMessages : ApplicationCommandsModule
    {
        [Event(DiscordEvent.MessageCreated)]
        public async Task NewMessage(DiscordClient sender, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot) return;

            var msg = e.Message.Content;

            var emoteNikifor = DiscordGuildEmoji.FromName(sender, ":nikifor:");
            var p = DiscordEmoji.FromUnicode(sender, "🇵");
            var i = DiscordEmoji.FromUnicode(sender, "🇮");
            var z = DiscordEmoji.FromUnicode(sender, "🇿");
            var d = DiscordEmoji.FromUnicode(sender, "🇩");
            var a = DiscordEmoji.FromUnicode(sender, "🇦");
            var b = DiscordEmoji.FromUnicode(sender, "🇧");
            var l = DiscordEmoji.FromUnicode(sender, "🇱");
            var y = DiscordEmoji.FromUnicode(sender, "🇾");
            var t = DiscordEmoji.FromUnicode(sender, "🇹");

            var lowerMsg = msg.ToLower().TrimEnd('!', ',', '.', '?', ' ');
            if (lowerMsg.TrimStart(' ').Equals("да") || lowerMsg.TrimStart(' ').Equals("da") ||
                lowerMsg.EndsWith(" да") || lowerMsg.EndsWith(" da"))
            {
                await e.Message.CreateReactionAsync(p);
                await e.Message.CreateReactionAsync(i);
                await e.Message.CreateReactionAsync(z);
                await e.Message.CreateReactionAsync(d);
                await e.Message.CreateReactionAsync(a);
            }
            if (msg.Split(' ').Any(a => a == ":nikifor:"))
            {
                await e.Message.CreateReactionAsync(b);
                await e.Message.CreateReactionAsync(l);
                await e.Message.CreateReactionAsync(y);
                await e.Message.CreateReactionAsync(a);
                await e.Message.CreateReactionAsync(t);
            }
            if (e.Message.MentionedRoles.Any(a => a.Id == 1135601767271374848))
            {
                var rnd = new Random();
                var embed = new DiscordEmbedBuilder()
                    .WithColor(new DiscordColor((byte)rnd.Next(0, 256), (byte)rnd.Next(0, 256), (byte)rnd.Next(0, 256)))
                    .WithTitle("Ссылка на дурачка, для дурачка")
                    .WithUrl("https://durak.rstgames.com/play/")
                    .Build();

                await e.Message.RespondAsync(embed);
            }
            if (e.Message.Author.Id == 660180184238129157)
            {
                await e.Message.CreateReactionAsync(emoteNikifor);
            }
        }
    }
}
