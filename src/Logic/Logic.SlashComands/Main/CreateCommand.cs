using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace Logic.SlashComands.Main
{
    public class CreateCommand : ApplicationCommandsModule
    {
        [SlashCommand("create", "Создать персональную комнату")]
        public async Task Create(InteractionContext ctx,
            [Option("name", "Название комнаты")]
            string? roomName = null,
            [Option("size", "Кол-во слотов")]
            int? size = null)
        {
            var client = ctx.Client;
            if (!client.TryGetChannel(1141802005405376552, out var channel)) return;
            if (!client.TryGetChannel(657266086323945496, out var lastChannel)) return;

            var newChannel = await channel.CloneAsync();
            await newChannel.ModifyAsync(cfg =>
            {
                cfg.Name = roomName ?? ctx.User.Username + " room";
                cfg.Type = ChannelType.Voice;
                cfg.UserLimit = size ?? 0;
                cfg.PermissionOverwrites = new List<DiscordOverwriteBuilder>()
                {
                    new DiscordOverwriteBuilder(ctx.UserId, OverwriteType.Member)
                    .Allow(Permissions.All)
                    .Remove(Permissions.BanMembers)
                };
                cfg.Position = lastChannel.Position - 1;
            });

            await ctx.DeleteResponseAsync();
        }
    }
}
