using Core.Interfaces;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Lavalink;
using Logic.SlashComands.Attributes;

namespace Logic.SlashComands.Music
{
    public class LeaveCommand : ApplicationCommandsModule
    {
        private readonly ILavaQueue _queue;

        public LeaveCommand(ILavaQueue queue)
        {
            _queue = queue;
        }

        [InVoice]
        [InMusicChannel]
        [Running]
        [SlashCommand("leave", "Закончить проигрывать музыку")]
        public async Task Leave(InteractionContext ctx)
        {
            var player = ctx.Client.GetLavalink().ConnectedSessions.First().Value?.GetGuildPlayer(ctx.Guild);

            if (player is null) return;

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

            await _queue.Disconnect(player);

            await ctx.DeleteResponseAsync();
        }
    }
}
