using Core.Interfaces;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Interactivity.Extensions;
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.Enums;
using Logic.SlashComands.Attributes;

namespace Logic.SlashComands.Music
{
    public enum SourceType
    {
        Auto,
        YouTube,
        YandexMusic
    }


    public class PlayCommand : ApplicationCommandsModule
    {
        private readonly ILavaQueue _queue;

        public PlayCommand(ILavaQueue queue)
        {
            _queue = queue;
        }

        [InVoice]
        [InMusicChannel]
        [HaveNode]
        [SlashCommand("play", "Заказать песню")]
        public async Task Play(InteractionContext ctx,
            [Option("поиск", "Поиск по названию песни или ссылке")]
            string search,
            [Option("источник", "Для ссылок использовать Auto")]
            SourceType sourceType = SourceType.Auto)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedSessions.FirstOrDefault().Value;
            var player = node.GetGuildPlayer(ctx.Guild);
            var channel = ctx.Member?.VoiceState?.Channel;
            if (player != null && player.Channel != channel)
            {
                var embed = new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Red)
                    .WithDescription("Я нахожусь в другом аудиоканале!")
                    .Build();

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(embed));

                return;
            }

            LavalinkGuildPlayer? lavaPlayer = player ?? await node.ConnectAsync(channel);

            if (lavaPlayer?.Player is null)
            {
                var embed = new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Red)
                    .WithDescription("Не удалось подключиться к голосовому каналу")
                    .Build();

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(embed));
                return;
            }

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("Ищем..."));

            var loadResult = await lavaPlayer.LoadTracksAsync(ResolveSongType(search, sourceType), search);
            LavalinkTrack track = new();
            List<LavalinkTrack> tracks = new();
            var loadType = loadResult.LoadType;
            try
            {
                if (loadResult.LoadType == LavalinkLoadResultType.Track)
                {
                    track = loadResult.GetResultAs<LavalinkTrack>();
                }
                else if (loadResult.LoadType == LavalinkLoadResultType.Playlist)
                {
                    tracks = loadResult.GetResultAs<LavalinkPlaylist>().Tracks;
                }
                else if (loadResult.LoadType == LavalinkLoadResultType.Search)
                {
                    var searchResult = loadResult.GetResultAs<List<LavalinkTrack>>();
                    var searchResultString = "";
                    var resultstrings = new List<string>();

                    for (int i = 0; i < Math.Min(searchResult.Count, 10); i++)
                    {
                        var length = searchResult[i].Info.Length;
                        var stringLength = $"{length.Hours.ToString("00")}:{length.Minutes.ToString("00")}:{length.Seconds.ToString("00")}";
                        var title = searchResult[i].Info.Title;
                        title = title.Length > 85 ? title.Substring(0, 85) : title;

                        searchResultString +=
                            $"{i + 1}. [{title}]({searchResult[i].Info.Uri}). [{stringLength}]" + Environment.NewLine;
                        resultstrings.Add(
                            $"{i + 1}. {title} [{stringLength}]");
                    }

                    var options = new List<DiscordStringSelectComponentOption>();
                    for (int i = 0; i < resultstrings.Count; i++)
                    {
                        options.Add(new DiscordStringSelectComponentOption(resultstrings[i], i.ToString()));
                    }

                    var rnd = new Random();
                    var generatedId = Guid.NewGuid().ToString();
                    var select = new DiscordStringSelectComponent("Выбери песню", options, generatedId);
                    var embed = new DiscordEmbedBuilder()
                        .WithTitle("Результат поиска")
                        .WithDescription(searchResultString)
                        .WithColor(new DiscordColor((byte)rnd.Next(0, 256), (byte)rnd.Next(0, 256), (byte)rnd.Next(0, 256)))
                        .Build();
                    var msg = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed)
                        .AddComponents(select));

                    var interactivity = ctx.Client.GetInteractivity();
                    var result = await interactivity.WaitForSelectAsync(msg, ctx.User, generatedId,
                    ComponentType.StringSelect, TimeSpan.FromMinutes(1));
                    if (result.TimedOut)
                    {
                        await ctx.DeleteResponseAsync();
                        return;
                    }

                    var selectedOption = result.Result.Values.First();
                    track = searchResult[int.Parse(selectedOption)];
                }
                else if (loadResult.LoadType is LavalinkLoadResultType.Error or LavalinkLoadResultType.Empty)
                {
                    throw new InvalidOperationException($"Не удалось ничего найти по данному запросу");
                }
                else
                {
                    throw new InvalidOperationException("Неизвестная ошибка");
                }
            }
            catch (FileNotFoundException)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Ничего не удалось найти!"));
            }
            catch (InvalidOperationException)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Ничего не удалось найти!"));
            }

            bool isPlaying = lavaPlayer.CurrentTrack is not null;
            if (isPlaying)
            {
                if (loadType is LavalinkLoadResultType.Track or LavalinkLoadResultType.Search)
                {
                    _queue.Queue.Enqueue((track, ctx.User));
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().WithContent($"Песня **{track.Info.Title}** добавлена в очередь. {Environment.NewLine}Позиция: {_queue.Queue.Count()}"));
                }
                else if (loadType is LavalinkLoadResultType.Playlist)
                {
                    foreach (var item in tracks)
                    {
                        _queue.Queue.Enqueue((item, ctx.User));
                    }

                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().WithContent($"Плейлист из {tracks.Count} песен успешно добавлен в очередь"));
                }
                return;
            }

            if (loadType is LavalinkLoadResultType.Track or LavalinkLoadResultType.Search)
            {
                await lavaPlayer.PlayAsync(track);
                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder().WithContent($"Запускаю **{track.Info.Title}**"));
            }

            if (loadType is LavalinkLoadResultType.Playlist)
            {
                await lavaPlayer.PlayAsync(tracks[0]);
                foreach (var item in tracks.Skip(1))
                {
                    _queue.Queue.Enqueue((item, ctx.User));
                }

                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder().WithContent($"Запускаю **{tracks[0].Info.Title}** и {tracks.Count - 1} песен добавлено в очередь"));
            }

            RegisterPlaybackFinishedEvent(lavaPlayer, ctx);

        }

        public void RegisterPlaybackFinishedEvent(LavalinkGuildPlayer player, InteractionContext ctx)
        {
            if (player?.CurrentTrack is null)
            {
                return;
            }

            player.TrackEnded += (sender, e) => _queue.TrackEnd(sender, e, ctx);
        }

        private static LavalinkSearchType ResolveSongType(string query, SourceType userSourceType)
        {
            if (userSourceType == SourceType.Auto)
            {
                if (RegexTemplates.YouTubeUrl.IsMatch(query))
                {
                    return LavalinkSearchType.Plain;
                }

                if (RegexTemplates.YandexMusic.IsMatch(query))
                {
                    return LavalinkSearchType.YandexMusic;
                }

                if (RegexTemplates.Url.IsMatch(query))
                {
                    return LavalinkSearchType.Plain;
                }

                return LavalinkSearchType.Youtube;
            }

            return SearchType(userSourceType);
        }

        private static LavalinkSearchType SearchType(SourceType sourceType)
        {
            switch (sourceType)
            {
                case SourceType.YouTube:
                    return LavalinkSearchType.Youtube;
                case SourceType.YandexMusic:
                    return LavalinkSearchType.YandexMusic;
                default:
                    return LavalinkSearchType.Youtube;
            }
        }
    }
}
