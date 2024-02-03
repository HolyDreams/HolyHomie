using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;

namespace Logic.SlashComands.Attributes
{
    public class Running : ApplicationCommandCheckBaseAttribute
    {
        public override async Task<bool> ExecuteChecksAsync(BaseContext ctx)
        {
            var player = ctx.Client.GetLavalink().ConnectedSessions.First().Value.GetGuildPlayer(ctx.Guild);
            if (player is null)
            {
                var embed = new DiscordEmbedBuilder()
                    .WithTitle("Я ничего не играю!")
                    .WithColor(DiscordColor.Red)
                    .Build();

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(embed));

                return false;
            }

            return true;
        }
    }
}
