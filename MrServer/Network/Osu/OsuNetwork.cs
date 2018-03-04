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

        public static async Task<OsuUser> DownloadOsuUser(string userName, byte gameMode = 0, int maxAttempts = 1) => 
            await DownloadOsuUserMain(userName, gameMode, maxAttempts);

        public static async Task<OsuUser> DownloadOsuUser(ulong userID, byte gameMode = 0, int maxAttempts = 1) => 
            await DownloadOsuUserMain(userID, gameMode, maxAttempts);

        ///<summary>Download x amount of top scores for a specified beatmap. Limit range 1 - 100.</summary>
        public static async Task<OsuScore[]> DownloadOsuBeatmapScores(OsuBeatmap beatmap, int gameMode, int scoreCount, int maxAttempts = 1) =>
            await OsuScore.CreateScores(NetworkHandler.DownloadJSON(
                $"{api}get_scores?" +
                $"k={Keys.API_KEYS["Osu"]}&" +
                $"b={beatmap.BeatmapID}&" +
                $"limit={scoreCount}&" +
                $"&m={gameMode}", maxAttempts).GetAwaiter().GetResult());

        ///<summary>Download the best play for a specified beatmap.</summary>
        public static async Task<OsuScore> DownloadOsuBeatmapBest(OsuBeatmap beatmap, OsuGameModes gameMode, ulong? user, int maxAttempts = 1) =>
            await DownloadOsuObject<OsuScore>(
                $"{api}get_scores?" +
                $"k={Keys.API_KEYS["Osu"]}&" +
                $"b={beatmap.BeatmapID}&" +
                $"limit=1&" +
                $"&m={OsuGameModesConverter.ToOfficialNumeration(gameMode)}" +
                string.Format("{0}", user.HasValue ? $"&u={user.Value}" : string.Empty), maxAttempts);

        public static async Task<OsuBeatmap> DownloadOsuBeatmap(int ID, bool group, int gameMode = -1, int maxAttempts = 2) =>
            await DownloadOsuObject<OsuBeatmap>(
                $"{api}get_beatmaps?" +
                $"k={Keys.API_KEYS["Osu"]}&" +
                $"{string.Format("{0}", group ? "s" : "b")}={ID}" +
                string.Format("{0}", gameMode != -1 ? $"&a=1&m={gameMode}" : string.Empty), maxAttempts);

        private static async Task<OsuUser> DownloadOsuUserMain(object user, byte gameMode, int maxAttempts) =>
            await DownloadOsuObject<OsuUser>(
                $"{api}get_user?" +
                $"k={Keys.API_KEYS["Osu"]}&" +
                $"u={user}&m={gameMode}", maxAttempts, gameMode);

        private static async Task<T> DownloadOsuObject<T>(string url, int maxAttempts, object additional = null)
        {
            string json = await NetworkHandler.DownloadJSON(url, maxAttempts);

            return (T)(additional != null ? Activator.CreateInstance(typeof(T), json, additional) : Activator.CreateInstance(typeof(T), json));
        }
    }
}
