using Core.Interfaces;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Lavalink;
using DisCatSharp.Enums;
using Logic.SlashComands.Attributes;

namespace Logic.SlashComands.Music
{
    public class SkipCommand : ApplicationCommandsModule
    {
        private readonly ILavaQueue _queue;

        public SkipCommand(ILavaQueue queue)
        {
            _queue = queue;
        }

        [InVoice]
        [InMusicChannel]
        [Running]
        [SlashCommand("skip", "Пропустить текущую песню")]
        public async Task Skip(InteractionContext ctx,
            [Option("кол-во", "Количество треков для скипа")]
            int count = 1)
        {
            var player = ctx.Client.GetLavalink().ConnectedSessions.First().Value?.GetGuildPlayer(ctx.Guild);
            var channel = ctx.Member?.VoiceState?.Channel;

            if (player?.Channel.Id != channel?.Id)
            {
                var embed = new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Red)
                    .WithTitle("Ты находишься в другом голосовом канале!")
                    .Build();

                await ctx.CreateResponseAsync(DisCatSharp.Enums.InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(embed));

                return;
            }

            if (_queue.Queue.Count == 0)
            {
                await player.DisconnectAsync();
            }
            else
            {
                if (count < 1)
                    count = 1;

                var track = _queue.Queue.Dequeue();
                for (int i = 0; i < count - 1; i++) 
                { 
                    track = _queue.Queue.Dequeue();
                }

                await player.PlayAsync(track.Item1);

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent($"Пропущено {count} песен"));

                await Task.Delay(15000);

                await ctx.DeleteResponseAsync();
            }

            return;
        }
    }
}
