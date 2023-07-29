using Discord.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Data;

namespace HolyHomie
{
    internal class CheckBGRank
    {
        public async void Start()
        {
            var checkTask = new Task(() => check(), Program.cts.Token);
            checkTask.Start();
        }
        List<BGRankStruct> bgRankList = new List<BGRankStruct>();
        List<BGRankStruct> bgRankListold = new List<BGRankStruct>();
        List<BGRankStruct> bgRankListEU = new List<BGRankStruct>();
        List<BGRankStruct> bgRankListEUold = new List<BGRankStruct>();
        bool needCheckRank = false;
        public void check()
        {
            while (0 == 0)
            {
                if (DateTime.Now.Minute % 10 == 0 || DateTime.Now.Minute == 0)
                {
                    int page = 1;
                    while (bgRankListEU.Count < 1000)
                    {
                        int seasonid = 9;

                        var json = new WebClient().DownloadString("https://hearthstone.blizzard.com/en-us/api/community/leaderboardsData?region=EU&leaderboardId=battlegrounds&seasonId=9&page=" + page);
                        var Rows = JObject.Parse(json)["leaderboard"]["rows"];
                        foreach (var item in Rows)
                        {
                            var text = item.ToString();
                            var res = JsonSerializer.Deserialize<BGRankStruct>(text);
                            bgRankListEU.Add(res);
                        }
                        page++;
                    }

                    for (int i = 0; i < bgRankListEU.Count; i++)
                    {
                        var sqlQuery = $@"INSERT INTO HSBGdaily
                                          VALUES ('{bgRankListEU[i].accountid}',
                                                  (SELECT Rating - {bgRankListEU[i].rating}
                                                   FROM HSBGeu
                                                   WHERE Name = '{bgRankListEU[i].accountid}' AND
                                                         (Rank >= {bgRankListEU[i].rank - 15} OR
                                                          Rank <= {bgRankListEU[i].rank + 15});

                                          UPDATE HSBGeu
                                          SET Name = '{bgRankListEU[i].accountid}',
                                              Rating = {bgRankListEU[i].rating}
                                          WHERE Rank = {bgRankListEU[i].rank};";
                        SQLRequest.SQLite(sqlQuery);
                    }
                }
                else if (DateTime.Now.Minute % 5 == 0 && DateTime.Now.Minute % 10 != 0)
                {
                    bgRankList.Clear();
                    var page = 1;
                    while (bgRankList.Count < 1000)
                    {
                        int seasonid = 9;

                        var json = new WebClient().DownloadString("https://hearthstone.blizzard.com/en-us/api/community/leaderboardsData?region=US&leaderboardId=battlegrounds&seasonId=9&page=" + page);
                        var Rows = JObject.Parse(json)["leaderboard"]["rows"];
                        foreach (var item in Rows)
                        {
                            var text = item.ToString();
                            var res = JsonSerializer.Deserialize<BGRankStruct>(text);
                            bgRankList.Add(res);
                        }
                        page++;
                    }


                    for (int i = 0; i < bgRankList.Count; i++)
                    {
                        var sqlQuery = $@"UPDATE HSBGus
                                          SET Name = '{bgRankList[i].accountid}',
                                              Rating = '{bgRankList[i].rating}'
                                          WHERE Rank = '{bgRankList[i].rank}'";
                        SQLRequest.SQLite(sqlQuery);
                    }
                }
                else if (DateTime.Now.Hour == 5 && DateTime.Now.Minute < 5)
                {
                    var sqlQuery = $@"DELETE 
                                      FROM HSBGdaily";
                    SQLRequest.SQLite(sqlQuery);
                }
                Task.Delay(59000).Wait();

            }
        }
    }
}
