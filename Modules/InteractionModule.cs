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
            Random rnd = new Random();
            string result = "Орёл!";
            if (rnd.Next(1, 100) < 51)
                result = "Решка!";
            await RespondAsync(result);

            Console.WriteLine($"{timeNow} {Context.User.Username} использовал команду '/flip'");

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
        [SlashCommand("socials", "Ссылки всех социальных сетей")]
        public async Task SocialAsync()
        {
            var embed = new EmbedBuilder()
                .WithColor(new Color(35, 215, 173))
                //.WithTitle("Ссылки на соцсети пожилого стримера:")
                .AddField("♿", "[Twitch](https://www.twitch.tv/fritterus)", true)
                .AddField("⏯️", "[YouTube](https://www.youtube.com/user/fritterus)", true)
                .AddField("✈️️", "[Telegram](https://t.me/fritterustv)", true)
                .AddField("📷", "[Instagram](https://instagram.com/fritterustv)", true)
                .AddField("🇧", "[VK](https://vk.com/fritterustv)", true)
                .AddField("💦", "[OnlyFans](https://youtu.be/dQw4w9WgXcQ)", true)
                .Build();

            await RespondAsync(embed: embed);
            Console.WriteLine($"{timeNow} {Context.User.Username} использовал команду '/socials'");
        }
        [SlashCommand("casino", "Испытай свою удачу")]
        public async Task CasinoAsync()
        {
            string[] casinoRoll = new string[5];
            casinoRoll[0] = ":sauropod:";
            casinoRoll[1] = ":doughnut:";
            casinoRoll[2] = ":eggplant:";
            casinoRoll[3] = ":hibiscus:";
            casinoRoll[4] = ":pig:";
            Random rnd = new Random();
            var res1 = casinoRoll[rnd.Next(0, casinoRoll.Length - 1)];
            var res2 = casinoRoll[rnd.Next(0, casinoRoll.Length - 1)];
            var res3 = casinoRoll[rnd.Next(0, casinoRoll.Length - 1)];
            var user = Context.User;
            var userName = user.Username + (user.Discriminator == "0000" ? "" : "#" + user.Discriminator);
            int countTry = 1;
            string sqlQuery = $@"SELECT TryNumber
                                 FROM CasinoResult
                                 WHERE UserName = '{userName}'";
            var res = SQLRequest.SQLite(sqlQuery);
            if (res.Rows.Count > 0)
            {
                var result = int.Parse(res.Rows[0][0].ToString());
                countTry = result + 1;
            }
            else
            {
                sqlQuery = $@"INSERT INTO CasinoResult (UserName, TryNumber)
                              VALUES ('{userName}', 0);

                              INSERT INTO SumResults (UserName, CountRuns, BestChance, WorstChance)
                              VALUES ('{userName}', 0, 9999, 0);";
                SQLRequest.SQLite(sqlQuery);
            }


            if (res1 == res2 && res2 == res3)
            {
                await RespondAsync($"{res1} {res2} {res3}");
                var message = await Context.Channel.GetMessagesAsync().FlattenAsync();


                sqlQuery = $@"UPDATE CasinoResult
                              SET TryNumber = 0
                              WHERE UserName = '{userName}';

                              SELECT CountRuns,
                                     BestChance,
                                     WorstChance
                              FROM SumResults
                              WHERE UserName = '{userName}'";
                res = SQLRequest.SQLite(sqlQuery);
                var resList = (from DataRow a in res.Rows
                               select new
                               {
                                   CountRuns = int.Parse(a["CountRuns"].ToString()),
                                   Best = int.Parse(a["BestChance"].ToString()),
                                   Worst = int.Parse(a["WorstChance"].ToString())
                               }).ToList().First();
                sqlQuery = $@"UPDATE SumResults
                              SET CountRuns = {resList.CountRuns + 1}{(resList.Best > countTry ? $@",
                                  BestChance = {countTry}" : "")}{(resList.Worst < countTry ? $@",
                                  WorstChance = {countTry}" : "")}
                              WHERE UserName = '{userName}'";
                SQLRequest.SQLite(sqlQuery);
            }
            else
            {
                var embed = new EmbedBuilder()
                                .WithColor(Color.Red)
                                .WithAuthor(userName, user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
                                .AddField("Результат:", $"{res1} {res2} {res3}", true)
                                .Build();
                await RespondAsync(embed: embed);

                sqlQuery = $@"UPDATE CasinoResult
                              SET TryNumber = {countTry++}
                              WHERE UserName = '{userName}';

                              UPDATE SumResults
                              SET CountRuns = (SELECT CountRuns
                                               FROM SumResults
                                               WHERE UserName = '{userName}') + 1
                              WHERE UserName = '{userName}';";
                SQLRequest.SQLite(sqlQuery);
                await Task.Delay(1000);
                var messages = await Context.Channel.GetMessagesAsync(1).FlattenAsync();
                await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);
            }
        }
        [SlashCommand("casinostat", "Показывает статистику казино")]
        public async Task CasinoStatAsync(SocketGuildUser пользователь = null)
        {
            string sqlQuery;
            if (пользователь == null)
            {
                sqlQuery = $@"SELECT UserName,
                                     CountRuns
                              FROM SumResults 
                              ORDER BY CountRuns DESC
                              LIMIT 5";
                var res = SQLRequest.SQLite(sqlQuery);
                if (res.Rows.Count == 0)
                {
                    await RespondAsync("Нет результатов, покрути рулетку, стань первым!");
                    return;
                }
                else
                {
                    var countList = (from DataRow a in res.Rows
                                    select new
                                    {
                                        UserName = (string)a["UserName"],
                                        CountRuns = int.Parse(a["CountRuns"].ToString()),
                                    }).ToList();
                    sqlQuery = $@"SELECT UserName,
                                         BestChance,
                                  FROM SumResults
                                  ORDER BY BestChance
                                  LIMIT 5";
                    res = SQLRequest.SQLite(sqlQuery);
                    var bestList = (from DataRow a in res.Rows
                                    select new
                                    {
                                        UserName = (string)a["UserName"],
                                        BestChance = int.Parse(a["BestChance"].ToString())
                                    }).ToList();
                    sqlQuery = $@"SELECT UserName,
                                         WorstChance
                                  FROM SumResults
                                  ORDER BY WorstChance DESC
                                  LIMIT 5";
                    res = SQLRequest.SQLite(sqlQuery);
                    var worstList = (from DataRow a in res.Rows
                                    select new
                                    {
                                        UserName = (string)a["UserName"],
                                        WorstChance = int.Parse(a["WorstChance"].ToString())
                                    }).ToList();

                    var embed = new EmbedBuilder()
                                .WithColor(new Color(35, 215, 173))
                                .AddField("Всего попыток:", $@"{string.Join(Environment.NewLine, countList.Select(a => a.UserName + " " + a.CountRuns + "попыток"))}", true)
                                .AddField("Самые везучие:", $@"{string.Join(Environment.NewLine, bestList.Select(a => a.UserName + " " + a.BestChance + "попыток"))}", true)
                                .AddField("Самые невезучие:", $@"{string.Join(Environment.NewLine, worstList.Select(a => a.UserName + " " + a.WorstChance + "попыток"))}", true)
                                .WithFooter($"По запросу: {Context.User.Username}{(Context.User.Discriminator == "0000" ? "" : $@"#{Context.User.Discriminator}")}", Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
                                .Build();

                    await RespondAsync(embed: embed);
                }
            }
            else
            {
                var userName = пользователь.Username + (пользователь.Discriminator == "0000" ? "" : "#" + пользователь.Discriminator);
                sqlQuery = $@"SELECT *
                              FROM SumResults
                              WHERE UserName = '{userName}'";
                var res = SQLRequest.SQLite(sqlQuery);
                if (res.Rows.Count == 0)
                {
                    await RespondAsync("Нет результатов, этот пользователь ещё не крутил рулетку!");
                    return;
                }
                else
                {
                    var resList = (from DataRow a in res.Rows
                                   select new
                                   {
                                       UserName = (string)a["UserName"],
                                       CountRuns = int.Parse(a["CountRuns"].ToString()),
                                       BestChance = int.Parse(a["BestChance"].ToString()),
                                       WorstChance = int.Parse(a["WorstChance"].ToString())
                                   }).ToList().First();
                    var embed = new EmbedBuilder()
                                .WithColor(new Color(35, 215, 173))
                                .AddField("Всего попыток:", $@"{resList.CountRuns} попыток", true)
                                .AddField("Лучшая попытка:", $@"С {resList.BestChance} раза", true)
                                .AddField("Худшая попытка:", $@"С {resList.WorstChance} Раза", true)
                                .WithFooter($"По запросу: {Context.User.Username}{(Context.User.Discriminator == "0000" ? "" : $@"#{Context.User.Discriminator}")}", Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
                                .Build();

                    await RespondAsync(embed: embed);
                }
            }
        }
        [SlashCommand("bgrank", "Посмотреть ранк хсбг")]
        public async Task BGRankCheck(string Никнейм = null, string Сервер = "EU")
        {
            string sqlQuery;
            int server;
            if (Сервер.ToLower() == "eu" || Сервер.ToLower() == "еу" || Сервер.ToLower() == "европа" || Сервер.ToLower() == "europa")
            {
                server = 0;
            }
            else if (Сервер.ToLower() == "us" || Сервер.ToLower() == "am" || Сервер.ToLower() == "юс" || Сервер.ToLower() == "usa" || Сервер.ToLower() == "америка" || Сервер.ToLower() == "america")
            {
                server = 1;
            }
            else
            {
                await RespondAsync("Вы ввели неправильный сервер, попробуйте EU или US");
                return;
            }

            if (Никнейм == null)
            {
                sqlQuery = $@"SELECT Name,
                                     Rank,
                                     Rating
                              FROM HSBG{(server == 0 ? "eu" : "us")}
                              Where Rank <= 50";
                var res = SQLRequest.SQLite(sqlQuery);
                var resList = (from DataRow a in res.Rows
                               select new
                               {
                                   Name = (string)a["Name"],
                                   Rank = int.Parse(a["Rank"].ToString()),
                                   Rating = int.Parse(a["Rating"].ToString())
                               }).OrderBy(q => q.Rank).ToList();

                var embed = new EmbedBuilder()
                            .WithColor(Color.Purple)
                            .AddField("ТОП 50:", $@"{string.Join(Environment.NewLine, resList.Select(a => a.Name + " " + a.Rank + " " + a.Rating))}", true)
                            .WithFooter($"По запросу: {Context.User.Username}{(Context.User.Discriminator == "0000" ? "" : $@"#{Context.User.Discriminator}")}", Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
                            .Build();

                await RespondAsync(embed: embed);
            }
            else
            {
                sqlQuery = $@"SELECT *
                              FROM HSBG{(server == 0 ? "eu" : "us")}
                              WHERE LOWER(Name) = '{Никнейм.ToLower()}'";
                var res = SQLRequest.SQLite(sqlQuery);
                if (res.Rows.Count == 0)
                {
                    await RespondAsync("Данный пользователь не найден! Возможно допущена ошибка или данного человека нет в топ 1000");
                    return;
                }
                else
                {
                    var resList = (from DataRow a in res.Rows
                                   select new
                                   {
                                       Name = (string)a["Name"],
                                       Rank = int.Parse(a["Rank"].ToString()),
                                       Rating = int.Parse(a["Rating"].ToString())
                                   }).ToList().First();

                    var embed = new EmbedBuilder()
                                .WithColor(Color.Purple)
                                .AddField("Игрок:", Никнейм, true)
                                .AddField("Ранк:", resList.Rank.ToString(), true)
                                .AddField("Рейтинг:", resList.Rating.ToString(), true)
                                .WithFooter($"По запросу: {Context.User.Username}{(Context.User.Discriminator == "0000" ? "" : $@"#{Context.User.Discriminator}")}", Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
                                .Build();

                    await RespondAsync(embed: embed);
                }
            }
        }
        [SlashCommand("bgdaily", "Посмотреть историю игр за день (обновляется в 5:00)")]
        public async Task BGDaily(string ИмяПользователя)
        {
            if (ИмяПользователя == null)
                return;

            var sqlQuery = $@"SELECT Rating
                              FROM HSBGdaily
                              WHERE LOWER(Name) = '{ИмяПользователя.ToLower()}'";
            var res = SQLRequest.SQLite(sqlQuery);
            if (res.Rows.Count < 1)
            {
                await RespondAsync("Нет результатов, возможно ошибка в никнейме или не сегодня не было игр!");
                return;
            }

            var resList = (from DataRow a in res.Rows
                           select new
                           {
                               Rating = int.Parse(a["Rating"].ToString()) > 0 ? "+" + a["Rating"].ToString() : a["Rating"].ToString()
                           }).ToList();

            var embed = new EmbedBuilder()
                            .WithColor(Color.DarkBlue)
                            .AddField(ИмяПользователя, $@"{string.Join("; ", resList.Select(a => a.Rating))}", true)
                            .WithFooter($"По запросу: {Context.User.Username}{(Context.User.Discriminator == "0000" ? "" : $@"#{Context.User.Discriminator}")}", Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
                            .Build();

            await RespondAsync(embed: embed);
        }
    }
}
