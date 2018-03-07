using MrServer.Additionals.Storing;
using MrServerPackets.ApiStructures.Osu;
using MrServerPackets.ApiStructures.Osu.Raw;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using MrServer.Bot.Client;
using System.Linq;
using MrServer.Bot.Commands;
using MrServer.Bot.Models;
using Newtonsoft.Json.Linq;
using System.Collections;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace MrServer.Network.Osu
{
    public class OsuNetwork
    {
        private DiscordClient discord;

        public OsuNetwork(DiscordClient discord)
        {
            this.discord = discord;

            discord.network.DiscordMessageReceived += Network_DiscordMessageReceived;
        }

        private void Network_DiscordMessageReceived(object sender, EventArgModels.DiscordMessageReceivedEventArgs e)
        {
            //Detect singular beatmaps
            Match match = Regex.Match(e.Message.Content, @"osu.ppy.sh\/b\/\d+(&m=\d)?");

            if (!match.Success) match = Regex.Match(e.Message.Content, @"osu.ppy.sh\/p\/beatmap\?b=\d+(&m=\d)?");

            if (match.Success)
            {
                MatchCollection parameters = Regex.Matches(e.Message.Content, @"\d+");

                string ID = parameters[0].Value;

                string gameMode = parameters.Count == 2 ? " " + parameters[1].Value : string.Empty;

                Console.WriteLine($"Beatmap detected, ID:{ID}");

                discord.cService.ExecuteAsync("Beatmap", ID + gameMode, e.Message, internally: true);
            }
        }

        private const string api = "https://osu.ppy.sh/api/";

        public static async Task<OsuUser> DownloadUser(string userName, OsuGameModes gameMode, SocketUserMessage logger = null, bool tolerateNull = false, int maxAttempts = 1) =>
            await DownloadUserMain(userName, gameMode, logger, tolerateNull, maxAttempts);

        public static async Task<OsuUser> DownloadUser(ulong userID, OsuGameModes gameMode, SocketUserMessage logger = null, bool tolerateNull = false, int maxAttempts = 1) =>
            await DownloadUserMain(userID, gameMode, logger, tolerateNull, maxAttempts);

        public static async Task<OsuUserRecent> DownloadUserRecent(ulong userID, OsuGameModes gameMode, SocketUserMessage logger = null, bool tolerateNull = false, int maxAttempts = 1) =>
            (await DownloadObjects<OsuUserRecent>(
                $"{api}get_user_recent?" +
                $"k={Additionals.Storing.Keys.API_KEYS["Osu"]}&" +
                $"m={OsuGameModesConverter.ToOfficialNumeration(gameMode)}&" +
                $"u={userID}&" +
                $"type=id&" +
                $"limit=1", maxAttempts, logger, tolerateNull: tolerateNull, additional: gameMode))[0];

        public static async Task<OsuScore[]> DownloadBeatmapBest(OsuBeatmap beatmap, OsuGameModes gameMode, ulong? user = null, int? scoreCount = null, bool tolerateNull = false, SocketUserMessage logger = null, int maxAttempts = 1) =>
            await DownloadObjects<OsuScore>(
                $"{api}get_scores?" +
                $"k={Additionals.Storing.Keys.API_KEYS["Osu"]}&" +
                $"b={beatmap.BeatmapID}" +
                string.Format("&limit={0}", scoreCount.HasValue ? $"{scoreCount.Value}" : "1") +
                $"&m={OsuGameModesConverter.ToOfficialNumeration(gameMode)}" +
                string.Format("{0}", user.HasValue ? $"&u={user.Value}" : string.Empty), maxAttempts, logger, tolerateNull: tolerateNull, additional: gameMode);

        public static async Task<OsuBeatmap> DownloadBeatmap(int ID, OsuGameModes gameMode, SocketUserMessage logger = null, bool tolerateNull = false, int maxAttempts = 2) =>
            (await DownloadObjects<OsuBeatmap>(
                $"{api}get_beatmaps?" +
                $"k={Additionals.Storing.Keys.API_KEYS["Osu"]}&" +
                $"b={ID}" +
                string.Format("{0}", gameMode != OsuGameModes.None ? $"&a=1&m={OsuGameModesConverter.ToOfficialNumeration(gameMode)}" : string.Empty), maxAttempts, logger, tolerateNull: tolerateNull))[0];

        public static async Task<OsuBeatmap[]> DownloadBeatmapPack(int ID, int? limit = 15, SocketUserMessage logger = null, bool tolerateNull = false, int maxAttempts = 2) =>
            await DownloadObjects<OsuBeatmap>(
                $"{api}get_beatmaps?" +
                $"k={Additionals.Storing.Keys.API_KEYS["Osu"]}&" +
                $"s={ID}&" +
                $"limit={limit}", maxAttempts, logger, tolerateNull: tolerateNull);

        private static async Task<OsuUser> DownloadUserMain(object user, OsuGameModes gameMode, SocketUserMessage logger, bool tolerateNull, int maxAttempts) =>
            (await DownloadObjects<OsuUser>(
                $"{api}get_user?" +
                $"k={Additionals.Storing.Keys.API_KEYS["Osu"]}&" +
                $"u={user}&m={OsuGameModesConverter.ToOfficialNumeration(gameMode)}", maxAttempts, logger, tolerateNull:tolerateNull, additional: gameMode))[0];


        //To be remade
        public static bool ReplayExists(OsuScore score)
        {
            //https://osu.ppy.sh/web/osu-getreplay.php?c={boundBestScore.ScoreID}&m={OsuGameModesConverter.ToOfficialNumeration(gameMode)}

            /*IWebDriver driver = new ChromeDriver();

            driver.Navigate().GoToUrl("https://osu.ppy.sh/");

            driver.FindElement(By.Id("login-open-button")).Click();

            driver.FindElement(By.Id("username-field"))*/


            return false;
        }

        private static async Task<T[]> DownloadObjects<T>(string url, int maxAttempts, SocketUserMessage logger, bool tolerateNull = false, object additional = null)
        {
            string json = await NetworkHandler.DownloadJSON(url, maxAttempts);

            try
            {
                object[] rawTypes = JsonConvert.DeserializeObject<object[]>(json);

                if (rawTypes.Length == 0)
                {
                    if (!tolerateNull) throw new ArgumentNullException();
                    else return new T[1]; //so it can return null when obtaining a singular object with [0]
                }

                T[] converted = new T[rawTypes.Length];

                for (int i = 0; i < rawTypes.Length; i++) converted[i] = (T)(additional != null ?
                        Activator.CreateInstance(typeof(T), rawTypes[i], additional) :
                        Activator.CreateInstance(typeof(T), rawTypes[i]));

                return converted;
            }
            catch (ArgumentNullException e)
            {
                if (logger != null)
                {
                    if(typeof(OsuUser) == typeof(T)) await logger.EditAsync(
                        "User was unable to be retrieved from the official osu! api. ❔\n" +
                        "Possible instances: bound user, top 3 players of a beatmap", null);
                    if (typeof(OsuBeatmap) == typeof(T)) await logger.EditAsync("This beatmap(or beatmap pack) was unable to be retrieved from the official osu! api. ❔", null);
                    else if (typeof(OsuUserRecent) == typeof(T)) await logger.EditAsync($"No recent plays have been found for game mode {OsuGameModesConverter.GameModeName((OsuGameModes)additional)}. 🔎", null);
                    else if (typeof(OsuScore) == typeof(T)) await logger.EditAsync($"Scores were unable to be retrieved from the official osu! api. 🔎", null);

                    throw e;
                }
            }

            return default(T[]);
        }
    }
}
