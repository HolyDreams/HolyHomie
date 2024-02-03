using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.EventArgs;

namespace Core.Interfaces
{
    public interface ILavaQueue
    {
        Queue<(LavalinkTrack, DiscordUser)> Queue { get; set; }

        Task TrackEnd(LavalinkGuildPlayer sender, LavalinkTrackEndedEventArgs e, InteractionContext ctx);
        Task Disconnect(LavalinkGuildPlayer player);
    }
}
