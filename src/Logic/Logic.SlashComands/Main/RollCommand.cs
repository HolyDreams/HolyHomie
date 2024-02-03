using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Enums;
using DisCatSharp.Entities;

namespace Logic.SlashComands.Main
{
    public class RollCommand : ApplicationCommandsModule
    {
        [SlashCommand("roll", "Кинуть кубик")]
        public async Task Roll(InteractionContext ctx,
            [Option("min", "Минимальное значение кубика")]
            int min = 0,
            [Option("max", "Максимальное значение кубика")]
            int max = 100)
        {
            if (min > max)
            {
                max = min + 1;
            }
            if (max < min)
            {
                min = max - 1;
            }

            var rnd = new Random();
            var result = rnd.Next(min, max + 1);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent(result.ToString()));
        }
    }
}
