using MrServer.Additionals.Storing;
using MrServerPackets.ApiStructures.Osu;
using MrServerPackets.ApiStructures.Osu.Raw;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MrServer.Network.Osu
{
    public class OsuNetwork
    {
        public static async Task<OsuUser> DownloadOsuUser(string userName, byte gameMode = 0, int maxAttempts = 1) => await DownloadOsuUserFlexable(userName, gameMode, maxAttempts);
        public static async Task<OsuUser> DownloadOsuUser(ulong userID, byte gameMode = 0, int maxAttempts = 1) => await DownloadOsuUserFlexable(userID, gameMode, maxAttempts);

        private static async Task<OsuUser> DownloadOsuUserFlexable(object o, byte gameMode, int maxAttempts)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                for(int i = 0; i < maxAttempts; i++)
                {
                    try
                    {
                        string json = await httpClient.GetStringAsync($"https://osu.ppy.sh/api/get_user?k={Keys.API_KEYS["Osu"]}&u={o}&m={gameMode}");
                        json = json.Remove(json.Length - 1, 1).Remove(0, 1);

                        return new OsuUser(JsonConvert.DeserializeObject<OsuUserRaw>(json), gameMode);
                    }
                    catch (Exception e) { Console.WriteLine(e.Message); }
                }
            }

            return null;
        }
    }
}
