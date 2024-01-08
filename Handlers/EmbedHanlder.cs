using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria.Player;

namespace HolyHomie.Handlers
{
    public class EmbedHanlder
    {
        public static async Task<Embed> GetMusicEmbed(string title, params LavaTrack[] tracks)
        {
            var rnd = new Random();
            string trackList = "";

            for (int i = 0; i < tracks.Length; i++)
            {
                trackList += string.Join("", i + 1, ". ", tracks[i].Title, '\n');
            }

            trackList.TrimEnd('\n');

            return await Task.Run(() => new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(trackList)
                .WithColor(new Color(rnd.Next(255), rnd.Next(255), rnd.Next(255)))
                .Build());
        }

        public static async Task<Embed> GetMusicEmbed(string title, params string[] trackNames)
        {
            var rnd = new Random();
            string trackList = "";

            for (int i = 0; i < trackNames.Length; i++)
            {
                trackList += string.Join("", i + 1, ". ", trackNames[i], '\n');
            }

            trackList.TrimEnd('\n');

            return await Task.Run(() => new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(trackList)
                .WithColor(new Color(rnd.Next(255), rnd.Next(255), rnd.Next(255)))
                .Build());
        }
    }
}
