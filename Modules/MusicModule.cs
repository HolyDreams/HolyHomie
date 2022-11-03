using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HolyHomie.Services;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.RegularExpressions;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;
using Victoria.Resolvers;
using Victoria.Responses.Search;

namespace HolyHomie.Modules
{
    public class MusicModule : ModuleBase<SocketCommandContext>
    {
        private readonly LavaNode _lavaNode;
        private static bool repeat = false;
        private readonly MusicHandler _musicHandler;

        string timeNow = DateTime.Now.ToString("[HH:mm:ss]");

        public MusicModule(LavaNode lavaNode, MusicHandler musicHandler)
        {
            _lavaNode = lavaNode;
            _musicHandler = musicHandler;
            
        }
        private async Task ReplyQuickEmbedAsync(string message, Color color = default)
        {

            string[] urlAvatar = { "https://media.tenor.com/sv9DsEJe-AAAAAAC/vibe-cat.gif", "https://media.tenor.com/Jxbk24m0vV4AAAAd/vibe-rabbit.gif", "https://media.tenor.com/OxvEqKx7_ScAAAAC/jig-dance.gif", "https://media.tenor.com/TSLx3Kue8K4AAAAd/vibe-gif.gif", "https://media.tenor.com/w_5Q79dNCh8AAAAi/cat-vibe.gif", "https://media.tenor.com/j0qPYTg9a94AAAAi/duck-ducky.gif", "https://media.tenor.com/V1f62JgjcEsAAAAi/vibe-squirtle-tristan.gif", "https://media.tenor.com/-qh2j3-x9CcAAAAi/discord-animated-blob.gif" };
            string gifImage = urlAvatar[new Random().Next(0, urlAvatar.Length - 1)];


            var builder = new EmbedBuilder()
                .WithAuthor(Context.User)
                .WithDescription(message)
                .WithColor(new Color(69, 230, 255))
                .WithThumbnailUrl(gifImage);
            var embed = builder.Build();
            await ReplyAsync(embed: embed);
        }

        [Command("Join"), Alias("j", "о", "ощшт")]
        [Summary("Присоедениться боту в бар.")]
        public async Task JoinAsync()
        {
            if (_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("Я уже подключен к голосовому каналу!");
                return;
            }

            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null || voiceState?.VoiceChannel.Id != 810298193945428039)
            {
                await ReplyAsync("Ты не в баре!");
                return;
            }

            try
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                await ReplyAsync("Залетаю в бар, включать музыку!");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                await ReplyAsync("Ой, что то пошло не так");
            }

            Console.WriteLine($"{timeNow} {Context.User.Username} использовал команду 'join'");
        }
        [Command("Play"), Alias("p", ">", "add", "sr", "з", "ык", "фвв")]
        [Summary("Воспроизвести добавить в очередь песню.")]
        public async Task PlayAsync([Remainder] string searchQuery)
        {
            var voiceState = Context.User as IVoiceState;

            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                return;
            }

            if (voiceState?.VoiceChannel is not null && !_lavaNode.HasPlayer(Context.Guild) && voiceState?.VoiceChannel.Id == 810298193945428039)
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
            }

            var queries = searchQuery.Split(";");
            var player = _lavaNode.GetPlayer(Context.Guild);

            foreach (var query in queries)
            {
                SearchResponse searchResponse;
                if (Uri.TryCreate(query, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                {
                    searchResponse = await _lavaNode.SearchAsync(SearchType.Direct, query);
                }
                else searchResponse = await _lavaNode.SearchYouTubeAsync(query);
                if (searchResponse.Status == SearchStatus.LoadFailed ||
                    searchResponse.Status == SearchStatus.NoMatches)
                {
                    await ReplyAsync($"Не могу найти: `{searchQuery}`.");
                    Console.WriteLine($"{timeNow} {Context.User.Username} пытался заказать песню командой 'play', но ничего не нашлось");
                    return;
                }

                if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
                {
                    if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
                    {
                        foreach (var track in searchResponse.Tracks)
                        {
                            player.Queue.Enqueue(track);
                        }
                        
                        await ReplyQuickEmbedAsync($"Загруженно в плейлист {searchResponse.Tracks.Count} песен");
                        Console.WriteLine($"{timeNow} {Context.User.Username} добавил в плейлист {searchResponse.Tracks.Count} песен командой 'play'");
                    }
                    else
                    {
                        var track = searchResponse.Tracks.First();
                        player.Queue.Enqueue(track);
                        var builder = new EmbedBuilder()
                            .WithAuthor($"Добавлен: {track.Title}")
                            .WithColor(new Color(69, 230, 255))
                            .WithThumbnailUrl(await track.FetchArtworkAsync());
                        var embed = builder.Build();
                        await ReplyAsync(embed: embed);
                        Console.WriteLine($"{timeNow} {Context.User.Username} заказал песню {track.Title} командой 'play'");
                    }
                }
                else
                {
                    var track = searchResponse.Tracks.ToArray()[0];

                    if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
                    {
                        for (var i = 0; i < searchResponse.Tracks.Count; i++)
                        {
                            if (i == 0)
                            {
                                await player.PlayAsync(track);
                                var builder = new EmbedBuilder()
                                .WithAuthor($"Сейчас играет: {track.Title}")
                                .WithColor(new Color(69, 230, 255))
                                .WithThumbnailUrl(await track.FetchArtworkAsync());
                                var embed = builder.Build();
                                await ReplyAsync(embed: embed);
                            }
                            else
                            {
                                player.Queue.Enqueue(searchResponse.Tracks.ElementAt(i));
                            }
                        }

                        await ReplyQuickEmbedAsync($"Добавлено '{searchResponse.Tracks.Count}' песен.");
                        Console.WriteLine($"{timeNow} {Context.User.Username} заказал песню {track.Title} командой 'play'");
                    }
                    else
                    {
                        await player.PlayAsync(track);
                        var builder = new EmbedBuilder()
                                .WithAuthor($"Сейчас играет: {track.Title}")
                                .WithColor(new Color(69, 230, 255))
                                .WithThumbnailUrl(await track.FetchArtworkAsync());
                        var embed = builder.Build();
                        await ReplyAsync(embed: embed);
                        Console.WriteLine($"{timeNow} {Context.User.Username} заказал песню {track.Title} командой 'play'");
                    }
                }
            }
            
        }

        [Command("Skip"), Alias("s", ">>", "ы", "ылшз")]
        [Summary("Скип текущей песни.")]
        public async Task SkipAsync()
        {
            if (!await IsConnectedUserInTheSameVoiceChannel() || !await IsConnectedBot()) return;

            string[] urlAvatar = { "https://media.tenor.com/sv9DsEJe-AAAAAAC/vibe-cat.gif", "https://media.tenor.com/Jxbk24m0vV4AAAAd/vibe-rabbit.gif", "https://media.tenor.com/OxvEqKx7_ScAAAAC/jig-dance.gif", "https://media.tenor.com/TSLx3Kue8K4AAAAd/vibe-gif.gif", "https://media.tenor.com/w_5Q79dNCh8AAAAi/cat-vibe.gif", "https://media.tenor.com/j0qPYTg9a94AAAAi/duck-ducky.gif", "https://media.tenor.com/V1f62JgjcEsAAAAi/vibe-squirtle-tristan.gif", "https://media.tenor.com/-qh2j3-x9CcAAAAi/discord-animated-blob.gif" };
            string gifImage = urlAvatar[new Random().Next(0, urlAvatar.Length - 1)];

            var player = _lavaNode.GetPlayer(Context.Guild);
            if (player.Queue.Count == 0)
            {
                await player.StopAsync();
                Console.WriteLine($"{timeNow} {Context.User.Username} скипнул {player.Track.Title} командой 'skip'");
                return;
            }

            (var oldTrack, var currentTrack) = await player.SkipAsync();
            //await ReplyAsync(null, false,
            //     new EmbedBuilder()
            //     .WithColor(new Color(69, 230, 255))
            //     .WithAuthor($"Мы скипнули: {oldTrack.Title}")
            //     .WithTitle($"Чтобы поставить {currentTrack.Title}")
            //     .WithThumbnailUrl(gifImage)
            //     .Build());

            Console.WriteLine($"{timeNow} {Context.User.Username} скипнул {oldTrack.Title} командой 'skip'");
        }

        [Command("Pause"), Alias("ll")]
        [Summary("Ставит на паузу музыку.")]
        public async Task PauseAsync()
        {
            if (!await IsConnectedUserInTheSameVoiceChannel() || !await IsConnectedBot()) return;

            var player = _lavaNode.GetPlayer(Context.Guild);
            if (player.PlayerState == Victoria.Enums.PlayerState.Paused ||
                player.PlayerState == Victoria.Enums.PlayerState.Stopped)
            {
                await ReplyAsync("Музыка уже на паузе!");
                Console.WriteLine($"{timeNow} {Context.User.Username} поставил музыку на паузу командой 'pause', но она уже была на паузе");
                return;
            }

            await player.PauseAsync();
            await ReplyAsync("Музыка на паузе!");

            Console.WriteLine($"{timeNow} {Context.User.Username} поставил музыку на паузу командой 'pause'");
        }

        [Command("Resume"), Alias("r", "unpause", "к")]
        [Summary("Продолжаем играть музыку.")]
        public async Task ResumeAsync()
        {
            if (!await IsConnectedUserInTheSameVoiceChannel() || !await IsConnectedBot()) return;

            var player = _lavaNode.GetPlayer(Context.Guild);
            if (player.PlayerState == Victoria.Enums.PlayerState.Playing)
            {
                await ReplyAsync("Музыка уже играет!");
                Console.WriteLine($"{timeNow} {Context.User.Username} продолжил играть музыку командой 'resume', но она уже играла");
                return;
            }

            await player.ResumeAsync();
            await ReplyAsync("Музыка возоблена!");
            Console.WriteLine($"{timeNow} {Context.User.Username} продолжил играть музыку командой 'resume'");
        }

        [Command("Stop"), Alias("o", "ыещз")]
        [Summary("Остановить воспроизвезедние музыки.")]
        public async Task StopAsync()
        {
            if (!await IsConnectedUserInTheSameVoiceChannel() || !await IsConnectedBot()) return;

            var player = _lavaNode.GetPlayer(Context.Guild);
            if (player.PlayerState != Victoria.Enums.PlayerState.Playing)
            {
                await ReplyAsync("Музыка не играет!");
                Console.WriteLine($"{timeNow} {Context.User.Username} остановил музыку командой 'stop', но музыка не играла");
                return;
            }

            await player.StopAsync();
            await ReplyAsync("Остановили музыку!");
            Console.WriteLine($"{timeNow} {Context.User.Username} остановил музыку командой 'stop'");
        }

        [Command("Leave"), Alias("l", "д")]
        [Summary("Выйти из канала.")]
        public async Task LeaveAsync()
        {
            if (!await IsConnectedUserInTheSameVoiceChannel() || !await IsConnectedBot()) return;

            var voiceState = Context.User as IVoiceState;
            await _lavaNode.LeaveAsync(voiceState?.VoiceChannel);
            Console.WriteLine($"{timeNow} {Context.User.Username} выгнал бота командой 'leave'");
        }

        [Command("Clear"), Alias("c", "с")]
        [Summary("Очистить очередь")]
        public async Task ClearAsync()
        {
            if (!await IsConnectedUserInTheSameVoiceChannel() || !await IsConnectedBot()) return;

            _lavaNode.GetPlayer(Context.Guild).Queue.Clear();
            Console.WriteLine($"{timeNow} {Context.User.Username} очистил плейлист командой 'clear'");
        }

        [Command("Repeat"), Alias("rp", "кз")]
        [Summary("Повторяет текущую песню бесконечно, при повторном вводе отключает повтор.")]
        public async Task RepeatAsync()
        {
            if (!await IsConnectedUserInTheSameVoiceChannel() || !await IsConnectedBot()) return;

            if (repeat = !repeat)
            {
                await ReplyAsync("Я начинаю повторять...", false, await CreateEmbed(_lavaNode.GetPlayer(Context.Guild).Track));
                Console.WriteLine($"{timeNow} {Context.User.Username} включил повтор песни командой 'repeat'");
            }

            else
            {
                await ReplyAsync("Я заканчиваю повторять...", false, await CreateEmbed(_lavaNode.GetPlayer(Context.Guild).Track));
                Console.WriteLine($"{timeNow} {Context.User.Username} выключил повтор песни командой 'repeat'");
            }
        }

        [Command("Shuffle"), Alias("shufle", "shuf", "random", "rnd", "ыргаду", "ырга", "кфтвщь", "ктв")]
        [Summary("Перемешать очередь.")]
        public async Task ShuffleAsync()
        {
            if (!await IsConnectedUserInTheSameVoiceChannel() || !await IsConnectedBot()) return;

            _lavaNode.GetPlayer(Context.Guild).Queue.Shuffle();
            Console.WriteLine($"{timeNow} {Context.User.Username} перемешал плейлист командой 'shuffle'");
        }

        [Command("NowPlaying"), Alias("np", "now", "тз", "тщц")]
        [Summary("Показывает информацию о текущей песне.")]
        public async Task NPAsync()
        {
            if (!await IsConnectedUserInTheSameVoiceChannel() || !await IsConnectedBot()) return;

            var track = _lavaNode.GetPlayer(Context.Guild).Track;

            TimeSpan position = track.Position, duration = track.Duration;
            int posInBar = (int)((double)position.Ticks / duration.Ticks * 10.0);

            string radioButton = "🔘";
            string barChar = "▬";
            string bar = "";

            for (int i = 0; i < 10; i++)
                bar += (i == posInBar) ? radioButton : barChar;

            await ReplyAsync(null, false,
                new EmbedBuilder()
                .WithColor(new Color(69, 230, 255))
                .WithAuthor(track.Author)
                .WithTitle(track.Title)
                .WithUrl(track.Url)
                .WithThumbnailUrl(await track.FetchArtworkAsync())
                .WithDescription($"{position.ToString(@"hh\:mm\:ss")} {bar} {duration}")
                .WithFields(new EmbedFieldBuilder()
                {
                    Name = "Заказано",
                    IsInline = true,
                    Value = Context.User
                })
                .WithFooter(footer => footer.Text = "Наслаждаемся!")
                .Build());

            Console.WriteLine($"{timeNow} {Context.User.Username} запросил название песни {track.Title} командой 'nowplaying'");
        }

        [Command("Search"), Alias("lf", "да", "ыуфкср")]
        [Summary("Ищет песню и показывает лучшие результаты.")]
        public async Task SearchAsync([Remainder] string query)
        {
            if (!await IsConnectedUserInTheSameVoiceChannel() || !await IsConnectedBot()) return;
            string[] urlAvatar = { "https://media.tenor.com/sv9DsEJe-AAAAAAC/vibe-cat.gif", "https://media.tenor.com/Jxbk24m0vV4AAAAd/vibe-rabbit.gif", "https://media.tenor.com/OxvEqKx7_ScAAAAC/jig-dance.gif", "https://media.tenor.com/TSLx3Kue8K4AAAAd/vibe-gif.gif", "https://media.tenor.com/w_5Q79dNCh8AAAAi/cat-vibe.gif", "https://media.tenor.com/j0qPYTg9a94AAAAi/duck-ducky.gif", "https://media.tenor.com/V1f62JgjcEsAAAAi/vibe-squirtle-tristan.gif", "https://media.tenor.com/-qh2j3-x9CcAAAAi/discord-animated-blob.gif" };
            string gifImage = urlAvatar[new Random().Next(0, urlAvatar.Length - 1)];

            await _lavaNode.SearchAsync(SearchType.YouTube, query).ContinueWith(async (searchResponse) =>
            {
                string tracks = "";

                foreach (var item in searchResponse.Result.Tracks)
                    tracks += item.Title + '\n';

                await ReplyAsync("Песни найдены на ютубе:", false, new EmbedBuilder()
                .WithColor(new Color(69, 230, 255))
                .WithDescription(tracks)
                .WithThumbnailUrl(gifImage)
                .Build());
            });

            Console.WriteLine($"{timeNow} {Context.User.Username} искал результаты по запросу {query} командой 'search'");
        }

        [Command("Queue"), Alias("q", "й")]
        [Summary("Показывает очередь песен.")]
        public async Task QueueAsync()
        {
            if (!await IsConnectedUserInTheSameVoiceChannel() || !await IsConnectedBot()) return;
            string[] urlAvatar = { "https://media.tenor.com/sv9DsEJe-AAAAAAC/vibe-cat.gif", "https://media.tenor.com/Jxbk24m0vV4AAAAd/vibe-rabbit.gif", "https://media.tenor.com/OxvEqKx7_ScAAAAC/jig-dance.gif", "https://media.tenor.com/TSLx3Kue8K4AAAAd/vibe-gif.gif", "https://media.tenor.com/w_5Q79dNCh8AAAAi/cat-vibe.gif", "https://media.tenor.com/j0qPYTg9a94AAAAi/duck-ducky.gif", "https://media.tenor.com/V1f62JgjcEsAAAAi/vibe-squirtle-tristan.gif", "https://media.tenor.com/-qh2j3-x9CcAAAAi/discord-animated-blob.gif" };
            string gifImage = urlAvatar[new Random().Next(0, urlAvatar.Length - 1)];

            string q = "";
            var queue = _lavaNode.GetPlayer(Context.Guild).Queue.ToArray();
            for (int i = 1; i <= queue.Length; i++)
            {
                q += i + ". " + queue[i - 1].Title + "\n";
            }

            await ReplyAsync("", false,
                new EmbedBuilder
                {
                    Title = "Очередь:",
                    Description = q
                }
                .WithColor(new Color(69, 230, 255))
                .WithThumbnailUrl(gifImage)
                .Build());

            Console.WriteLine($"{timeNow} {Context.User.Username} запросил плейлист командой 'queue'");
        }

        [Command("Help"), Alias("All", "h", "р")]
        [Summary("Показывает все команды.")]
        public async Task HelpAsync()
        {
            string all = "";
            foreach (var command in MusicHandler._cmdService?.Commands.ToArray())
            {
                string aliases = "";
                for (int i = 1; i < command.Aliases.Count; i++)
                {
                    aliases += command.Aliases[i];

                    if (i != command.Aliases.Count - 1)
                        aliases += ", ";
                }

                if (aliases == "")
                    all += $"**{command.Name}** - *{command.Summary}*\n";
                else
                    all += $"**{command.Name}** (**{aliases}**) - *{command.Summary}*\n";
            }

            await ReplyAsync(null, false,
                new EmbedBuilder()
                .WithColor(new Color(69, 230, 255))
                .WithAuthor("Команды для бота")
                .WithTitle("Чтобы вызвать команду в данном текстовом канале напишите 'команда'")
                .WithDescription(all)
                .WithFooter(footer => footer.Text = "Наслаждаемся!")
                .Build());

            Console.WriteLine($"{timeNow} {Context.User.Username} запросил список команд командой 'help'");
        }

        public async Task<bool> IsConnectedBot()
        {
            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("Я не в баре!");
                Console.WriteLine($"{timeNow} {Context.User.Username} пытался отправить команды музыкальному боту, но он был не в баре");
                return false;
            }

            return true;
        }

        public async Task<bool> IsConnectedUserInTheSameVoiceChannel()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel != _lavaNode.GetPlayer(Context.Guild).VoiceChannel || voiceState?.VoiceChannel == null || voiceState?.VoiceChannel.Id != 810298193945428039)
            {
                await ReplyAsync("Ты не в баре!");
                Console.WriteLine($"{timeNow} {Context.User.Username} пытался пригласить музыкального бота, но он был не в баре");
                return false;
            }

            return true;
        }

        public async Task<Embed> CreateEmbed(LavaTrack track)
        {
            return new EmbedBuilder()
                .WithColor(new Color(69, 230, 255))
                .WithAuthor(track.Author)
                .WithTitle(track.Title)
                .WithUrl(track.Url)
                .WithThumbnailUrl(await track.FetchArtworkAsync())
                .WithFields(new EmbedFieldBuilder()
                {
                    Name = "Длительность",
                    IsInline = true,
                    Value = track.Duration
                })
                .WithFields(new EmbedFieldBuilder()
                {
                    Name = "Заказан",
                    IsInline = true,
                    Value = (Context != null) ? Context.User.ToString() : Context.User
                })
                .WithFooter(footer => footer.Text = "Наслаждаемся!")
                .Build();
        }
    }
}