using Discord;
using Discord.Commands;
using Victoria;
using Victoria.EventArgs;
using Victoria.Responses.Search;

namespace HolyHomie.Modules
{
    public class MusicModule : ModuleBase<SocketCommandContext>
    {
        private readonly LavaNode _lavaNode;
        private static bool repeat = false;

        public MusicModule(LavaNode lavaNode)
        {
            _lavaNode = lavaNode;
            _lavaNode.OnTrackEnded += OnTrackEndedAsync;
        }

        private async Task OnTrackEndedAsync(TrackEndedEventArgs arg)
        {
            if (arg.Player.PlayerState == Victoria.Enums.PlayerState.Stopped)
            {
                if (repeat)
                    await arg.Player.PlayAsync(arg.Track);
                else
                {
                    if (arg.Player.Queue != null && arg.Player.Queue.Count > 0)
                    {
                        await arg.Player.SkipAsync();
                        await arg.Player.TextChannel.SendMessageAsync("Сейчас играет...", false, await CreateEmbed(arg.Player.Track));
                    }
                }
            }
        }

        [Command("Join"), Alias("j")]
        [Summary("Присоедениться боту в бар.")]
        public async Task JoinAsync()
        {
            if (_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("Я уже подключен к голосовому каналу!");
                return;
            }

            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null) return;

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
        }
        [Command("Play"), Alias("p", ">", "add", "sr")]
        [Summary("Воспроизвести добавить в очередь песню.")]
        public async Task PlayAsync([Remainder] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                await ReplyAsync("Очередь пуста");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
                await JoinAsync();

            var searchResponse = await _lavaNode.SearchYouTubeAsync(query);
            if (searchResponse.Status == SearchStatus.LoadFailed ||
                searchResponse.Status == SearchStatus.NoMatches)
            {
                searchResponse = await _lavaNode.SearchAsync(SearchType.YouTubeMusic, query);
                if (searchResponse.Status == SearchStatus.LoadFailed ||
                    searchResponse.Status == SearchStatus.NoMatches)
                {
                    await ReplyAsync($"Ничего не найдено для '{query}'.");
                    return;
                }
            }

            var player = _lavaNode.GetPlayer(Context.Guild);
            //var track = searchResponse.Tracks.ToArray()[0];
            if (player.PlayerState == Victoria.Enums.PlayerState.Playing ||
                player.PlayerState == Victoria.Enums.PlayerState.Paused)
            {
                if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
                {
                    foreach (var track in searchResponse.Tracks)
                    {
                        player.Queue.Enqueue(track);
                    }
                    await ReplyAsync($"Твоя песня уже в очереди, она {searchResponse.Tracks.Count}.");
                }
                else
                {
                    var track = searchResponse.Tracks.FirstOrDefault();
                    player.Queue.Enqueue(track);
                    await ReplyAsync($"{track.Title} добавлен в очередь.");
                }
            }
            else
            {
                {
                    var track = searchResponse.Tracks.ToArray()[0];

                    if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
                    {
                        for (var i = 0; i < searchResponse.Tracks.Count; i++)
                        {
                            if (i == 0)
                            {
                                await player.PlayAsync(track);
                                await ReplyAsync($"Сейчас играет: {track.Title}.");
                            }
                            else
                            {
                                player.Queue.Enqueue(searchResponse.Tracks.ToArray()[i]);
                            }
                        }
                        await ReplyAsync($"В очереди {searchResponse.Tracks.Count} песен.");
                    }
                    else
                    {
                        await player.PlayAsync(track);
                        await ReplyAsync(null, false, await CreateEmbed(track));
                    }
                }
            }
        }

        [Command("Skip"), Alias("s", ">>")]
        [Summary("Скип текущей песни.")]
        public async Task SkipAsync()
        {
            if (!await IsConnectedUserInTheSameVoiceChannel() || !await IsConnectedBot()) return;

            var player = _lavaNode.GetPlayer(Context.Guild);
            if (player.Queue.Count == 0)
            {
                await player.StopAsync();
                return;
            }

            (_, var currentTrack) = await player.SkipAsync();
            await ReplyAsync(null, false, await CreateEmbed(player.Track));
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
                return;
            }

            await player.PauseAsync();
            await ReplyAsync("Музыка на паузе!");
        }

        [Command("Resume"), Alias("r", "unpause")]
        [Summary("Продолжаем играть музыку.")]
        public async Task ResumeAsync()
        {
            if (!await IsConnectedUserInTheSameVoiceChannel() || !await IsConnectedBot()) return;

            var player = _lavaNode.GetPlayer(Context.Guild);
            if (player.PlayerState == Victoria.Enums.PlayerState.Playing)
            {
                await ReplyAsync("Музыка уже играет!");
                return;
            }

            await player.ResumeAsync();
            await ReplyAsync("Музыка возоблена!");
        }

        [Command("Stop"), Alias("o")]
        [Summary("Остановить воспроизвезедние музыки.")]
        public async Task StopAsync()
        {
            if (!await IsConnectedUserInTheSameVoiceChannel() || !await IsConnectedBot()) return;

            var player = _lavaNode.GetPlayer(Context.Guild);
            if (player.PlayerState != Victoria.Enums.PlayerState.Playing)
            {
                await ReplyAsync("Музыка не играет!");
                return;
            }

            await player.StopAsync();
            await ReplyAsync("Остановили музыку!");
        }

        [Command("Leave"), Alias("l")]
        [Summary("Выйти из канала.")]
        public async Task LeaveAsync()
        {
            if (!await IsConnectedUserInTheSameVoiceChannel() || !await IsConnectedBot()) return;

            var voiceState = Context.User as IVoiceState;
            await _lavaNode.LeaveAsync(voiceState?.VoiceChannel);
        }

        [Command("Clear"), Alias("c")]
        [Summary("Очистить очередь")]
        public async Task ClearAsync()
        {
            if (!await IsConnectedUserInTheSameVoiceChannel() || !await IsConnectedBot()) return;

            _lavaNode.GetPlayer(Context.Guild).Queue.Clear();
        }

        //[Command("Jump"), Alias("j")]
        //[Summary("Перепрыгнуть какое то количество песен. Нужно ввести количество.")]
        //public async Task JumpAsync(int amount)
        //{
        //    if (!await IsConnectedUserInTheSameVoiceChannel() || !await IsConnectedBot()) return;

        //    var player = _lavaNode.GetPlayer(Context.Guild);
        //    if (amount <= player.Queue.Count)
        //    {
        //        player.Queue.RemoveRange(0, amount - 1);

        //        if (player.Queue.Count > 0)
        //            await player.SkipAsync();
        //    }
        //    else
        //        await ReplyAsync("Неудача.");
        //}

        //[Command("Remove"), Alias("delete", "del")]
        //[Summary("")]

        [Command("Repeat"), Alias("rp")]
        [Summary("Повторяет текущую песню бесконечно, при повторном вводе отключает повтор.")]
        public async Task RepeatAsync()
        {
            if (!await IsConnectedUserInTheSameVoiceChannel() || !await IsConnectedBot()) return;

            if (repeat = !repeat)
                await ReplyAsync("Я начинаю повторять...", false, await CreateEmbed(_lavaNode.GetPlayer(Context.Guild).Track));
            else
                await ReplyAsync("Я заканчиваю повторять...", false, await CreateEmbed(_lavaNode.GetPlayer(Context.Guild).Track));
        }

        [Command("Shuffle"), Alias("shufle", "shuf", "random")]
        [Summary("Перемешать очередь.")]
        public async Task ShuffleAsync()
        {
            if (!await IsConnectedUserInTheSameVoiceChannel() || !await IsConnectedBot()) return;

            _lavaNode.GetPlayer(Context.Guild).Queue.Shuffle();
        }

        [Command("NowPlaying"), Alias("np", "now")]
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
        }

        [Command("Search"), Alias("lf")]
        [Summary("Ищет песню и показывает лучшие результаты.")]
        public async Task SearchAsync([Remainder] string query)
        {
            if (!await IsConnectedUserInTheSameVoiceChannel() || !await IsConnectedBot()) return;

            await _lavaNode.SearchAsync(SearchType.YouTube, query).ContinueWith(async (searchResponse) =>
            {
                string tracks = "";

                foreach (var item in searchResponse.Result.Tracks)
                    tracks += item.Title + '\n';

                await ReplyAsync("Песни найдены на ютубе:", false, new EmbedBuilder()
                .WithColor(new Color(69, 230, 255))
                .WithDescription(tracks)
                .WithFooter(footer => footer.Text = "Наслаждаемся!")
                .Build());
            });
        }

        //[Command("Lyrics"), Alias("karaoke", "text")]
        //[Summary("Показывает текст текущей песни.")]
        //public async Task LyricsAsync()
        //{
        //    if (!await IsConnectedUserInTheSameVoiceChannel() || !await IsConnectedBot()) return;

        //    var player = _lavaNode.GetPlayer(Context.Guild);
        //    if (player.PlayerState != Victoria.Enums.PlayerState.Playing)
        //    {
        //        await ReplyAsync("Я не играю никакой песни.");
        //        return;
        //    }

        //    var track = player.Track;
        //    var lyrics = await track.FetchLyricsFromOvhAsync();
        //    if (string.IsNullOrWhiteSpace(lyrics))
        //    {
        //        await ReplyAsync($"Я не нашёл текст для {track.Title}");
        //        return;
        //    }

        //    var splitLyrics = lyrics.Split('\n');
        //    var stringBuilder = new StringBuilder();
        //    foreach (var line in splitLyrics)
        //    {
        //        if (Enumerable.Range(1900, 2000).Contains(stringBuilder.Length))
        //        {
        //            await ReplyAsync($"```{stringBuilder}```");
        //            stringBuilder.Clear();
        //        }
        //        else
        //        {
        //            stringBuilder.AppendLine(line);
        //        }
        //    }

        //    await ReplyAsync($"'''{stringBuilder}'''");
        //}

        [Command("Queue"), Alias("q")]
        [Summary("Показывает очередь песен.")]
        public async Task QueueAsync()
        {
            if (!await IsConnectedUserInTheSameVoiceChannel() || !await IsConnectedBot()) return;

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
                .Build());
        }

        //[Command("Volume"), Alias("v")]
        //[Summary("Изменить общую громкость воспроизведения.")]
        //public async Task VolumeAsync(ushort volume)
        //{
        //    if (!await IsConnectedUserInTheSameVoiceChannel() || !await IsConnectedBot()) return;

        //    var voiceState = Context.User as IVoiceState;

        //    var player = _lavaNode.GetPlayer(Context.Guild);

        //    await player.UpdateVolumeAsync(volume);
        //    await ReplyAsync(null, false, 
        //        new EmbedBuilder()
        //        .WithColor(new Color(69, 230, 255))
        //        .WithTitle($"Громкость успешно изменена на {volume}")
        //        .Build());

        //}


        [Command("Help"), Alias("All", "h")]
        [Summary("Показывает все команды.")]
        public async Task HelpAsync()
        {
            string all = "";
            foreach (var command in Services.MusicHandler._cmdService?.Commands.ToArray())
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
                .WithTitle("Чтобы вызвать команду в данном текстовом канале напишите (! 'команда')")
                .WithDescription(all)
                .WithFooter(footer => footer.Text = "Наслаждаемся!")
                .Build());
        }

        public async Task<bool> IsConnectedBot()
        {
            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("Я не в баре!");
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