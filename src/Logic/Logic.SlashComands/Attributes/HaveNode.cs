using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Lavalink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.SlashComands.Attributes
{
    public class HaveNode : ApplicationCommandCheckBaseAttribute
    {
        public override async Task<bool> ExecuteChecksAsync(BaseContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedSessions.FirstOrDefault().Value;

            var result = node != null;

            if (!result)
            {
                await ctx.CreateResponseAsync(DisCatSharp.Enums.InteractionResponseType.ChannelMessageWithSource,
                    new DisCatSharp.Entities.DiscordInteractionResponseBuilder().WithContent("Нет подключения к серверу музыки, попробуйте позже"));
            }

            return result;
        }
    }
}
