using Discord.Commands;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Timers;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using System.Net.Http.Headers;
using Discord.Rest;
using System.Data;

namespace HolyHomie.Modules
{
    public class InteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        string timeNow = DateTime.Now.ToString("[HH:mm:ss]");
        [SlashCommand("roll", "Кидаете кубик")]
        public async Task HandleRollCommand(int min = 1, int max = 100)
        {
            if (min == null)
            {
                min = 1;
            }
            if (max == null)
            {
                max = 100;
            }
            Random rnd = new Random();
            await RespondAsync(Convert.ToString(rnd.Next(min, max + 1)));
            Console.WriteLine($"{timeNow} {Context.User.Username} использовал команду '/roll'");
        }

        [SlashCommand("flip", "Кидаете монетку")]
        public async Task HandleFlipCommand()
        {
            try
            {
                Random rnd = new Random();
                string result = "Орёл!";
                if (rnd.Next(1, 100) < 51)
                    result = "Решка!";
                await RespondAsync(result);

                Console.WriteLine($"{timeNow} {Context.User.Username} использовал команду '/flip'");
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync( ex.Message);
            }

        }
        [SlashCommand("avatar", "Показывает аватар пользователя")]
        public async Task AvatarAsync(SocketGuildUser пользователь = null)
        {
            if (пользователь == null)
            {
                пользователь = Context.User as SocketGuildUser;
            }
            Random rnd = new Random();
            Color rndColor = new Color(new Color(rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255)));

            var embed = new EmbedBuilder()
                .WithColor(rndColor)
                .WithAuthor($"{пользователь.Username}#{пользователь.Discriminator}", пользователь.GetAvatarUrl() ?? пользователь.GetDefaultAvatarUrl())
                .WithImageUrl(пользователь.GetAvatarUrl(ImageFormat.Auto, 2048) ?? пользователь.GetDefaultAvatarUrl())
                .WithFooter($"По запросу: {Context.User.Username}{(Context.User.Discriminator == "0000" ? "" : $@"#{Context.User.Discriminator}")}", Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
                .Build();

            await RespondAsync(embed: embed);
            Console.WriteLine($"{timeNow} {Context.User.Username} запросил аватар {пользователь.Username} командой '/avatar'");
        }
    }
}
