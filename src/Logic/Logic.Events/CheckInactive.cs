using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Lavalink;

namespace Logic.Lava.EvenHandlers
{
    [EventHandler]
    public class CheckInactive : ApplicationCommandsModule
    {
        [Event(DiscordEvent.VoiceStateUpdated)]
        public async Task VoiceStateUpdated(DiscordClient client, VoiceStateUpdateEventArgs e)
        {
            var player = client.GetLavalink()?.ConnectedSessions.First().Value?.GetGuildPlayer(e.Guild);

            if (e.User.Id == client.CurrentUser.Id && e.After.Channel is null)
            {
                _ = Task.Run(async () =>
                {
                    if (player is null) return;

                    await player.DisconnectAsync();
                });
            }
            else if (e.User.Id != client.CurrentUser.Id && e.Before.Channel != null && e.Before.Channel.Users.Count == 1) 
            {
                _ = Task.Run(async () =>
                {
                    if (e.Before.Channel.Users.First().Id != client.CurrentUser.Id || player.ChannelId != e.Before.Channel.Id) return;

                    await Task.Delay(TimeSpan.FromSeconds(30));
                    if (e.Before.Channel.Users.Count > 1) return;

                    await player.DisconnectAsync();
                });
            }
        }
    }
}
