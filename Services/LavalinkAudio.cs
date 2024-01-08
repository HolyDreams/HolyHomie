using Discord;
using Discord.WebSocket;
using HolyHomie.Handlers;
using HolyHomie.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Victoria.Node;
using Victoria.Node.EventArgs;
using Victoria.Player;
using Victoria.Responses.Search;

namespace HolyHomie.Services
{
    public class LavalinkAudio
    {
        private readonly LavaNode _lavaNode;
        private readonly ILogger<LavalinkAudio> _logger;

        public LavalinkAudio(LavaNode lavaNode, ILogger<LavalinkAudio> logger)
        {
            _lavaNode = lavaNode;
            _logger = logger;
            _lavaNode.OnTrackEnd += _lavaNode_OnTrackEnd;
        }

        private async Task _lavaNode_OnTrackEnd(TrackEndEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
        {
            var player = arg.Player;
            if (player.Vueue.Count == 0)
                await _lavaNode.LeaveAsync(player.VoiceChannel);
            else
                await arg.Player.PlayAsync(player.Vueue.FirstOrDefault());
            
        }

        public async Task<List<Tracks>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return null;

            try
            {
                var fullQuery = query;
                if (!Uri.TryCreate(query, UriKind.Absolute, out var uri))
                    fullQuery = "ytsearch: " + query;

                var search = await _lavaNode.SearchAsync(SearchType.Direct, fullQuery);

                if (search.Status == SearchStatus.NoMatches)
                {
                    return null;
                }

                var trackList = new List<Tracks>();
                foreach (var track in search.Tracks.Take(1)) 
                {
                    trackList.Add(new Tracks
                    {
                        Name = track.Title + " (" + track.Duration.Hours.ToString("00") + ":" + track.Duration.Minutes.ToString("00") + ":" + track.Duration.Seconds.ToString("00") + ")",
                        Url = track.Url
                    });
                }
                return trackList;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.Message);
                return null;
            }
        }

        public async Task<string> PlayAsync(IVoiceState voiceState, SocketGuildUser user, IGuild guild, string query)
        {
            if (user.VoiceChannel == null)
            {
                return "Сначало нужно зайти в канал, чтобы я смог присоединиться!";
            }

            if (!_lavaNode.HasPlayer(guild))
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel);
            }

            try
            {
                if (!_lavaNode.TryGetPlayer(guild, out var player))
                    throw new Exception("Не удалось создать плеер!");

                LavaTrack track;

                var search = Uri.IsWellFormedUriString(query, UriKind.Absolute) ?
                    await _lavaNode.SearchAsync(SearchType.YouTube, query) :
                    await _lavaNode.SearchAsync(SearchType.SoundCloud, query);

                if (search.Status == SearchStatus.NoMatches)
                {
                    return "Ничего не удалось найти по данному поиску";
                }

                track = search.Tracks.FirstOrDefault();

                if (track.Duration.TotalMinutes > 60)
                {
                    return "Заказан трек длительностью больше 60 минут!";
                };

                if (player.Track != null && player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused)
                {
                    player.Vueue.Enqueue(track);
                    return $"Песня {track.Title} добавлена в очередь!";
                }

                await player.PlayAsync(track);
                return $"Сейчас играет: {track.Title}.    Url: {track.Url}";
            }

            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.Message);
                return $"Ошибка: {ex.Message}";
            }

        }

        public async Task LeaveAsync(IGuild guild)
        {
            try
            {
                if (!_lavaNode.TryGetPlayer(guild, out var player))
                    throw new Exception("Не удалось создать плеер!");

                if (player.PlayerState is PlayerState.Playing)
                {
                    await player.StopAsync();
                }

                await _lavaNode.LeaveAsync(player.VoiceChannel);
            }

            catch (InvalidOperationException ex)
            {
                _logger.Log(LogLevel.Error, ex.Message);
            }
        }

        public async Task<Embed> ListAsync(IGuild guild)
        {
            try
            {
                if (!_lavaNode.TryGetPlayer(guild, out var player))
                    throw new Exception("Не удалось создать плеер!");

                if (player.PlayerState is PlayerState.Playing)
                    return await EmbedHanlder.GetMusicEmbed("Текущий плейлист:", player.Vueue.ToArray());
                else
                    return null;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.Message);
                return null;
            }

        }

        public async Task SkipTrackAsync(IGuild guild)
        {
            try
            {
                if (!_lavaNode.TryGetPlayer(guild, out var player))
                    throw new Exception("Не удалось создать плеер!");

                if (player.Vueue.Count > 0)
                    await player.SkipAsync();
                else
                    await _lavaNode.LeaveAsync(player.VoiceChannel);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.Message);
                return;
            }
        }

        public async Task<string> NowPlayingAsync(IGuild guild)
        {
            try
            {
                if (!_lavaNode.TryGetPlayer(guild, out var player))
                    throw new Exception("Не удалось создать плеер!");

                var track = player.Track;
                return "Сейчас играет: " + track.Author + " " + track.Title;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
