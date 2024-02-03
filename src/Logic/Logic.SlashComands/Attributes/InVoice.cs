using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;

namespace Logic.SlashComands.Attributes
{
    public class InVoice : ApplicationCommandCheckBaseAttribute
    {
        public override async Task<bool> ExecuteChecksAsync(BaseContext ctx)
        {
            if (ctx.Member.VoiceState?.Channel is null)
            {
                var embed = new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Red)
                    .WithDescription("Вы не подключены к голосовому каналу")
                    .Build();

                await ctx.CreateResponseAsync(DisCatSharp.Enums.InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(embed));

                return false;
            }

            return true;
        }
    }
}
