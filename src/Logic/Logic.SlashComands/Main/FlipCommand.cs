using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.SlashComands.Main
{
    public class FlipCommand : ApplicationCommandsModule
    {
        [SlashCommand("flip", "Кинуть монетку")]
        public async Task Flip (InteractionContext ctx)
        {
            var rnd = new Random();
            var resultInt = rnd.Next(0, 1000);

            var result = resultInt < 500 ? "Орёл" : "Решка";

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent(result));
        }
    }
}
