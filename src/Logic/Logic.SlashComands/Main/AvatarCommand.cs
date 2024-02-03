using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace Logic.SlashComands.Main
{
    public class AvatarCommand : ApplicationCommandsModule
    {
        [SlashCommand("avatar", "Показать аватарку другого пользователя")]
        public async Task Avatar(InteractionContext ctx,
            [Option("user", "Пользователь чью аватарку показать")]
            DiscordUser? targetUser = null)
        {
            var user = targetUser ?? ctx.User;

            var rnd = new Random();

            var embed = new DiscordEmbedBuilder()
                .WithColor(new DiscordColor((byte)rnd.Next(0, 256), (byte)rnd.Next(0, 256), (byte)rnd.Next(0, 256)))
                .WithAuthor($"{user.Username}", user.GetAvatarUrl(ImageFormat.Auto) ?? user.DefaultAvatarUrl)
                .WithImageUrl(user.GetAvatarUrl(ImageFormat.Auto, 2048) ?? user.DefaultAvatarUrl)
                .WithFooter($"По запросу: {ctx.User.Username}", ctx.User.GetAvatarUrl(ImageFormat.Auto) ?? ctx.User.DefaultAvatarUrl)
                .Build();

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }
    }
}
