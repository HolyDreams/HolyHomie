using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using Logic.SlashComands.Attributes;
using DisCatSharp.Lavalink;
using Core.Interfaces;

namespace Logic.SlashComands.Music
{
    public class QueueCommand : ApplicationCommandsModule
    {
        private readonly ILavaQueue _queue;

        public QueueCommand(ILavaQueue queue)
        {
            _queue = queue;
        }

        [InMusicChannel]
        [Running]
        [SlashCommand("playlist", "Показать текущий плейлист")]
        public async Task PlayList(InteractionContext ctx)
        {
            var player = ctx.Client.GetLavalink().ConnectedSessions.First().Value?.GetGuildPlayer(ctx.Guild);

            if (player is null) return;

            if (_queue.Queue.Count == 0)
            {
                var badEmbed = new DiscordEmbedBuilder()
                    .WithTitle("Очередь пуста")
                    .WithColor(DiscordColor.Red)
                    .Build();

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(badEmbed));

                return;
            }

            var rnd = new Random();

            var result = "";

            int i = 0;
            foreach (var track in _queue.Queue.ToList())
            {
                var length = track.Item1.Info.Length;
                var stringLength = $"{length.Hours.ToString("00")}:{length.Minutes.ToString("00")}:{length.Seconds.ToString("00")}";
                result += ++i + ". " + track.Item1.Info.Title + "  (" + stringLength + ")" + Environment.NewLine;
            }

            var embed = new DiscordEmbedBuilder()
                .WithTitle($"Песни в очереди:")
                .WithColor(new DiscordColor((byte)rnd.Next(0, 256), (byte)rnd.Next(0, 256), (byte)rnd.Next(0, 256)))
                .WithDescription(result)
                .Build();

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }
    }
}
