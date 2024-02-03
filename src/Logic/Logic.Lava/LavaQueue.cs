using Core.Interfaces;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.EventArgs;
using DisCatSharp.Lavalink.Enums;

namespace Logic.Lava
{
    public class LavaQueue : ILavaQueue
    {
        #region Private

        private Queue<(LavalinkTrack, DiscordUser)> queue = new ();

        #endregion

        #region Properties

        public Queue<(LavalinkTrack, DiscordUser)> Queue 
        { 
            get
            {
                return queue;
            }
            set
            {
                queue = value;
            }
        }

        #endregion

        #region Methods

        public async Task TrackEnd(LavalinkGuildPlayer sender, LavalinkTrackEndedEventArgs e, InteractionContext ctx)
        {
            if (e.Reason == LavalinkTrackEndReason.Replaced)
            {
                return;
            }

            if (queue.Count > 0)
            {
                var track = queue.Dequeue();
                await sender.PlayAsync(track.Item1);
            }

            if (sender.CurrentTrack == null && queue.Count == 0)
            {
                await Disconnect(sender);
            }
        }

        public async Task Disconnect(LavalinkGuildPlayer player)
        {
            queue.Clear();

            await player.DisconnectAsync();
        }

        #endregion
    }
}
