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
        private const string api = "https://osu.ppy.sh/api/";

        public static async Task<OsuUser> DownloadOsuUser(string userName, byte gameMode = 0, int maxAttempts = 1) => 
            await DownloadOsuUserMain(userName, gameMode, maxAttempts);

        public static async Task<OsuUser> DownloadOsuUser(ulong userID, byte gameMode = 0, int maxAttempts = 1) => 
            await DownloadOsuUserMain(userID, gameMode, maxAttempts);

        ///<summary>Download x amount of top scores for a specified beatmap. Limit range 1 - 100.</summary>
        public static async Task<OsuScore[]> DownloadOsuBeatmapScores(OsuBeatmap beatmap, int scoreCount, int maxAttempts = 1) =>
            await OsuScore.CreateScores(NetworkHandler.DownloadJSON($"{api}get_scores?k={Keys.API_KEYS["Osu"]}&b={beatmap.BeatmapID}&limit={scoreCount}&m={beatmap.GameModeRaw}", maxAttempts).GetAwaiter().GetResult());

        ///<summary>Download the best play for a specified beatmap.</summary>
        public static async Task<OsuScore> DownloadOsuBeatmapBest(OsuBeatmap beatmap, int maxAttempts = 1) =>
            await DownloadOsuObject<OsuScore>($"{api}get_scores?k={Keys.API_KEYS["Osu"]}&b={beatmap.BeatmapID}&limit=1&m={beatmap.GameModeRaw}", maxAttempts);

        public static async Task<OsuBeatmap> DownloadOsuBeatmap(int ID, bool group, int maxAttempts = 2) =>
            await DownloadOsuObject<OsuBeatmap>($"{api}get_beatmaps?k={Keys.API_KEYS["Osu"]}&{string.Format("{0}", group ? "s" : "b")}={ID}", maxAttempts);

        private static async Task<OsuUser> DownloadOsuUserMain(object user, byte gameMode, int maxAttempts) =>
            await DownloadOsuObject<OsuUser>($"{api}get_user?k={Keys.API_KEYS["Osu"]}&u={user}&m={gameMode}", maxAttempts, gameMode);

        private static async Task<T> DownloadOsuObject<T>(string url, int maxAttempts, object additional = null)
        {
            string json = await NetworkHandler.DownloadJSON(url, maxAttempts);

            return (T)(additional != null ? Activator.CreateInstance(typeof(T), json, additional) : Activator.CreateInstance(typeof(T), json));
        }
    }
}
