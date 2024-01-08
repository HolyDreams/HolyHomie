using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using HolyHomie.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HolyHomie.Modules
{
    public class AudioModule : InteractionModuleBase<SocketInteractionContext>
    {
        public LavalinkAudio _audio { get; set; }

        [SlashCommand("leave", "Покинуть голосовой чат")]
        public async Task Leave() =>
            await _audio.LeaveAsync(Context.Guild);

        [SlashCommand("play", "Включить песню")]
        public async Task Play([Remainder] string search) =>
            await ReplyAsync(await _audio.PlayAsync(Context.User as IVoiceState, Context.User as SocketGuildUser, Context.Guild, search));

        [SlashCommand("list", "Показать плейлист")]
        public async Task GetList() =>
            await ReplyAsync(embed: await _audio.ListAsync(Context.Guild));

        [SlashCommand("skip", "Пропустить текущую песню")]
        public async Task Skip() =>
            await _audio.SkipTrackAsync(Context.Guild);

        [SlashCommand("nowplay", "Какая песня сейчас играет")]
        public async Task NowPlaying() =>
            await ReplyAsync(await _audio.NowPlayingAsync(Context.Guild));
    }
}
