using MrServer.Additionals.Storing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MrServer.Network.WoW
{
    public static class WoWNetwork
    {
        public static async Task<string> DownloadBoss(int bossID) => await new HttpClient().GetStringAsync($"https://eu.api.battle.net/wow/boss/{bossID}?locale=en_GB&apikey={Keys.API_KEYS["BattleNet"]}");

        public static async Task<string> DownloadAuctions(string realm)
        {
            using (HttpClient http = new HttpClient())
            {
                string json = await http.GetStringAsync($"https://eu.api.battle.net/wow/auction/data/{realm}?locale=en_GB&apikey={Keys.API_KEYS["BattleNet"]}");

                try
                {
                    dynamic jsonObj = JObject.Parse(json);

                    string url = jsonObj.files[0].url;

                    return await http.GetStringAsync(url);
                }
                catch (Exception e) { throw e; }

            }
        }
    }
}
