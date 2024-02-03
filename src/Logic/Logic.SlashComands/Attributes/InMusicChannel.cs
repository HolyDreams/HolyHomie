using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;

namespace Logic.SlashComands.Attributes
{
    public class InMusicChannel : ApplicationCommandCheckBaseAttribute
    {
        public override async Task<bool> ExecuteChecksAsync(BaseContext ctx)
        {
            var musicChannel = ctx.ChannelId == 1115321643175260181;
            if (!musicChannel)
            {
                var channel = ctx.Guild.GetChannel(1115321643175260181);

                await ctx.CreateResponseAsync(DisCatSharp.Enums.InteractionResponseType.ChannelMessageWithSource,
                    new DisCatSharp.Entities.DiscordInteractionResponseBuilder().WithContent($"Команды для музыки работают только в канале: {channel.Name}"));

                await Task.Delay(15000);

                await ctx.DeleteResponseAsync();
            }

            return musicChannel;
        }
    }
}
