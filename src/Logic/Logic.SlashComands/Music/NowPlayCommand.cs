using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Lavalink;
using DisCatSharp.Enums;
using Logic.SlashComands.Attributes;
using NuGet.Packaging.Signing;

namespace Logic.SlashComands.Music
{
    public class NowPlayCommand : ApplicationCommandsModule
    {
        [InMusicChannel]
        [Running]
        [SlashCommand("now", "Показать какая песня сейчас играет")]
        public async Task NowPlay(InteractionContext ctx)
        {
            var player = ctx.Client.GetLavalink().ConnectedSessions.First().Value?.GetGuildPlayer(ctx.Guild);

            if (player is null) return;

            var track = player.CurrentTrack;

            if (track is null)
            {
                var badEmbed = new DiscordEmbedBuilder()
                    .WithTitle("Сейчас ничего не играет")
                    .WithColor(DiscordColor.Red)
                    .Build();

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(badEmbed));

                return;
            }

            var rnd = new Random();

            var totalLength = track.Info.Length;
            var curLength = player.Player.PlayerState.Position;

            var proc = (int)(totalLength / curLength) / 5;

            var playShow = "";

            for (int i = 0; i < 20; i++)
            {
                playShow += i == proc ? ">" : "-";
            }

            var embed = new DiscordEmbedBuilder()
                .WithTitle("Сейчас играет:")
                .WithColor(new DiscordColor((byte)rnd.Next(0, 256), (byte)rnd.Next(0, 256), (byte)rnd.Next(0, 256)))
                .WithDescription($@"**{track.Info.Title}**{Environment.NewLine} {Environment.NewLine}" +
                $"{GetTimeFromTimespan(curLength)}/{GetTimeFromTimespan(totalLength) + Environment.NewLine}" +
                $"{playShow}")
                .WithUrl(track.Info.Uri)
                .Build();

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }
        private string GetTimeFromTimespan(TimeSpan time)
        {
            return $"{time.Hours.ToString("00")}:{time.Minutes.ToString("00")}:{time.Seconds.ToString("00")}";
        }
    }
}
