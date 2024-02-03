using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Enums;

namespace Logic.Events
{
    [EventHandler]
    public class CheckNewUser : ApplicationCommandsModule
    {
        [Event(DiscordEvent.GuildMemberAdded)]
        public async Task NewUserJoin(DiscordClient sender, DisCatSharp.EventArgs.GuildMemberAddEventArgs e)
        {
            if (!e.Guild.Roles.TryGetValue(678975609447120927, out var role)) return;

            await e.Member.GrantRoleAsync(role);
        }
    }
}
