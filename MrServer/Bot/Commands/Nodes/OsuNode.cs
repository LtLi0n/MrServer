using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using MrServerPackets.ApiStructures.Osu;
using MrServerPackets.ApiStructures.Osu.Database;
using MrServerPackets.Discord.Entities;
using MrServerPackets.Discord.Models;
using MrServerPackets.Discord.Models.Guilds;

using MrServer.SQL.Osu;
using MrServer.Bot.Commands.Permissions;
using MrServer.Bot.Client;
using MrServer.Bot.Commands.Attributes;
using MrServer.Bot.Commands.Attributes.Permissions;
using MrServer.Bot.Models;
using MrServer.Network.Osu;
using MrServer.Additionals.Tools;
using MrServer.Network;

namespace MrServer.Bot.Commands.Nodes
{
    [CommandNode("Osu")]
    [RequireRole(417325871838265356)]
    class OsuNode : ICommandNode
    {
        private OsuSQL OsuDB => Program.Entry.DataBases.OsuDB;

        [Command("Osu")]
        public async Task Osu([Remainder]string userName = null)
        {
            if (Context.Message.Mentions.Length > 1)
            {
                await Context.Channel.SendMessageAsync("AAAA Too many mentions, calm down.\nOne at a time :)");
                return;
            }

            OsuBoundUserDB boundUser = string.IsNullOrEmpty(userName) ? boundUser = await OsuDB.GetBoundUserBy_DiscordID(Context.Message.Author.ID) : Context.Message.Mentions.Length == 1 ? await OsuDB.GetBoundUserBy_DiscordID(Context.Message.Mentions[0]) : await OsuDB.GetBoundUserBy_UserName(userName);

            if (Context.Message.Mentions.Length == 1 && boundUser == null)
            {
                await Context.Channel.SendMessageAsync("Mentioned user is not binded.");
                return;
            }

            if (boundUser != null) userName = boundUser.UserName;
            else if (string.IsNullOrEmpty(userName))
            {
                await Context.Channel.SendMessageAsync(
                    "You don't exist in the database yet." +
                    "Do `$osubind [username]` to continue the use of `$osu` without parameters.");
            }

            SocketUserMessage msg = await Context.Channel.SendMessageAsync("Fetching data...", attachID: true);

            OsuUser osuUser = await Network.Osu.OsuNetwork.DownloadUser(userName, maxAttempts: 2);

            EmbedBuilder eb = new EmbedBuilder();

            double progressDebug = osuUser.OsuLevel.Progress;

            eb.Description =
            $"*• PP:* __***{Math.Round(osuUser.PP, 2, MidpointRounding.AwayFromZero)}***__\n" +
            $"*• Accuracy:* __***{string.Format("{0:0.##}", osuUser.Accuracy)}%***__\n" +
            $"*• Level:* __***{osuUser.OsuLevel.Level}***__  ~~-~~ *{osuUser.OsuLevel.Progress.ToString("#0.000%")}*";

            if (boundUser != null)
            {
                //Get all entries of tracked user gameplay statistics
                OsuGameModes[] userGameModes = OsuGameModesConverter.ToGameModesArray(boundUser.GameModes);
                OsuGameModeUserDB[] gameModeUsers = new OsuGameModeUserDB[userGameModes.Length];
                for (int i = 0; i < userGameModes.Length; i++) gameModeUsers[i] = await OsuDB.GetGameModeUserBy_OsuID(boundUser.UserID, userGameModes[i]);

                if (userGameModes.Length > 0)
                {
                    for (int i = 0; i < userGameModes.Length; i++)
                    {
                        string emoji = OsuGameModesConverter.ToEmoji(userGameModes[i]);

                        eb.AddField(x =>
                        {
                            x.Name = $"{emoji} Total Hits {Enum.GetName(typeof(OsuGameModes), userGameModes[i])} {CustomEmoji.TotalHits_Anim}";

                            if (gameModeUsers[i] != null)
                            {
                                string hitsDaily = gameModeUsers[i].HitsDaily / 10000 > 0 ? gameModeUsers[i].HitsDaily.ToString("#,#", CultureInfo.InvariantCulture) : gameModeUsers[i].HitsDaily.ToString();
                                string hitsWeekly = gameModeUsers[i].HitsWeekly / 10000 > 0 ? gameModeUsers[i].HitsWeekly.ToString("#,#", CultureInfo.InvariantCulture) : gameModeUsers[i].HitsWeekly.ToString();
                                string hitsMonthly = gameModeUsers[i].HitsMonthly / 10000 > 0 ? gameModeUsers[i].HitsMonthly.ToString("#,#", CultureInfo.InvariantCulture) : gameModeUsers[i].HitsMonthly.ToString();
                                string hitsSince = gameModeUsers[i].HitsSince / 10000 > 0 ? gameModeUsers[i].HitsSince.ToString("#,#", CultureInfo.InvariantCulture) : gameModeUsers[i].HitsSince.ToString();
                                string totalHits = gameModeUsers[i].TotalHits / 10000 > 0 ? gameModeUsers[i].TotalHits.ToString("#,#", CultureInfo.InvariantCulture) : gameModeUsers[i].TotalHits.ToString();

                                x.Value =
                                $"*Today:* ***{hitsDaily}***\n" +
                                $"*This Week:* ***{hitsWeekly}***\n" +
                                $"*This Month:* ***{hitsMonthly}***\n" +
                                $"*Since:* ***{hitsSince}*** / ***{totalHits}***";
                            }
                            else
                            {
                                x.Value =
                                $"No stored data has been found yet.\n" +
                                $"Wait for the next update.\n" +
                                $"*maximum waiting time - 1 minute*";
                            }

                            if (gameModeUsers.Length > 2 && i < 2) x.Value += "\n\u200b";
                            else if (i == 0) x.Value += "\t\t\t\u200b";

                            x.IsInline = true;
                        });
                    }
                }
            }

            eb.WithAuthor(x =>
            {
                x.IconUrl = $"https://images-ext-2.discordapp.net/external/sBhebNjHsypWWjDEqpVLu303x-bnOQ7AlxIKozEXgtQ/https/cdn.discordapp.com/attachments/202046273052868608/355446459514093579/mode-osu-med.png";
                x.Name = $"{osuUser.Username}";
            });

            eb.Color = Color.LightPink;

            eb.Thumbnail = osuUser.AvatarURL;

            await msg.EditAsync("", eb.Build());
        }

        [Command("OsuUnTrack")]
        public async Task Osu_UnTrack(string gameModeStr)
        {
            OsuBoundUserDB boundUser = await OsuDB.GetBoundUserBy_DiscordID(Context.Message.Author.ID);

            if (boundUser == null) await Context.Channel.SendMessageAsync($"You need to bind your osu account first with `{Context.Discord.PREFIX}OsuBind [player_name]`.");
            else
            {
                OsuGameModes gameMode = OsuGameModesConverter.FromOfficialName(gameModeStr, true);

                if (boundUser.GameModes.HasFlag(gameMode))
                {
                    await OsuDB.RemoveOsuGameModeUser(boundUser.UserID, gameMode);
                    boundUser.GameModes ^= gameMode;

                    await OsuDB.UpdateBoundOsuUser(boundUser);

                    await Context.Channel.SendMessageAsync($"You have been successfully removed from {Enum.GetName(typeof(OsuGameModes), gameMode)} gameplay database!");
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"You are not in {Enum.GetName(typeof(OsuGameModes), gameMode)} gameplay tracked database.");
                }
            }
        }

        [Command("OsuTrack")]
        public async Task OsuTrack(string gameModeStr)
        {
            OsuBoundUserDB boundUser = await OsuDB.GetBoundUserBy_DiscordID(Context.Message.Author.ID);

            if (boundUser == null) await Context.Channel.SendMessageAsync($"You need to bind your osu account first with `{Context.Discord.PREFIX}OsuBind [player_name]`.");
            else
            {
                OsuGameModes gameMode = OsuGameModesConverter.FromOfficialName(gameModeStr, true);

                OsuGameModeUserDB gameModeUser = await OsuDB.GetGameModeUserBy_OsuID(boundUser.UserID, gameMode);

                if (!boundUser.GameModes.HasFlag(gameMode))
                {
                    OsuUser osuUser = await Network.Osu.OsuNetwork.DownloadUser(boundUser.UserID, OsuGameModesConverter.ToOfficialNumeration(gameMode));

                    await OsuDB.WriteOsuGameModeUser(osuUser, gameMode);

                    boundUser.GameModes |= gameMode;
                    await OsuDB.UpdateBoundOsuUser(osuUser, boundUser);

                    await Context.Channel.SendMessageAsync($"You are now being tracked for {Enum.GetName(typeof(OsuGameModes), gameMode)} gameplay.");
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"You are already being tracked for {Enum.GetName(typeof(OsuGameModes), gameMode)} gameplay.");
                }
            }
        }

        [Command("OsuBind")]
        public async Task OsuBindUser([Remainder]string userName)
        {
            OsuUser osuUser = await Network.Osu.OsuNetwork.DownloadUser(userName);

            OsuBoundUserDB BoundUser = await OsuDB.GetBoundUserBy_DiscordID(Context.Message.Author.ID);

            if (BoundUser == null)
            {
                await OsuDB.RegisterBoundOsuUser(osuUser, Context.Message.Author.ID);

                await Context.Channel.SendMessageAsync($"Binded {userName} to `{OsuSQL.table_name}`");
            }
            else
            {
                await Context.Channel.SendMessageAsync($"{userName} is already binded. Use OsuUnbind to free up the used slot.");
            }
        }

        [Command("OsuUnBind")]
        public async Task OsuUnbindUser([Remainder]string userName)
        {
            OsuBoundUserDB boundUser = await OsuDB.GetBoundUserBy_DiscordID(Context.Message.Author.ID);

            if (boundUser == null)
            {
                await Context.Channel.SendMessageAsync("Your bound account was not found in the database.");
            }
            else
            {
                OsuGameModes[] boundUserGamemodes = OsuGameModesConverter.ToGameModesArray(boundUser.GameModes);

                for (int i = 0; i < boundUserGamemodes.Length; i++)
                {
                    await OsuDB.RemoveOsuGameModeUser(boundUser.UserID, boundUserGamemodes[i]);
                }

                await OsuDB.RemoveBoundUser(Context.Message.Author.ID);

                await Context.Channel.SendMessageAsync($"UnBinded {boundUser.UserName} from `{OsuSQL.table_name}`.");
            }
        }

        [Command("Recent")]
        public async Task GetRecent([Remainder]string target = null)
        {
            SocketUserMessage msg = await Context.Channel.SendMessageAsync("Fetching data...", attachID: true);

            ulong? targetID = null;

            string username = string.Empty;
            string country = string.Empty;

            if (string.IsNullOrEmpty(target))
            {
                OsuBoundUserDB bound = await OsuDB.GetBoundUserBy_DiscordID(Context.Message.Author.ID);

                if (bound != null)
                {
                    targetID = bound.UserID;
                    country = bound.Country;
                    username = bound.UserName;
                }

                //Add "You are not bound"
            }
            else
            {
                if (target.Where(x => x < '0' || x > '9').Count() > 0)
                {
                    //Not ID
                }
                else targetID = ulong.Parse(target);
            }

            OsuUserRecent recent = await OsuNetwork.DownloadUserRecent(targetID.Value, OsuGameModes.STD);

            if(recent == null)
            {
                await msg.EditAsync("No recent plays have been found. 🔎", null);
                return;
            }

            OsuBeatmap beatmap = await OsuNetwork.DownloadBeatmap(recent.BeatmapID, false, 0);

            OsuUser beatmapCreator = await OsuNetwork.DownloadUser(beatmap.Creator);

            EmbedBuilder eb = beatmap.ToEmbedBuilder(OsuGameModes.STD, beatmapCreator, true);

            eb.Fields.Insert(0, new EmbedFieldBuilder
            {
                Name = $"Recent {OsuGameModesConverter.ToEmoji(OsuGameModes.STD)}",
                Value = ToScoreString(recent, country, OsuGameModes.STD, username, nameFormat: '\u200b')
            });

            await msg.EditAsync("", eb.Build());
        }

        [Command("Beatmap")]
        [Hidden]
        public async Task GetBeatmap(string ID, string mode = null, string action = null)
        {
            SocketUserMessage msg = await Context.Channel.SendMessageAsync("Fetching data...", attachID: true);

            int modeInt = (string.IsNullOrEmpty(mode) ? -1 : int.Parse(mode));

            OsuBeatmap beatmap = await OsuNetwork.DownloadBeatmap(int.Parse(ID), false, modeInt);

            OsuGameModes gameMode = modeInt == -1 ? beatmap.GameMode : OsuGameModesConverter.FromOfficialNumeration((byte)modeInt);

            OsuUser creator = await OsuNetwork.DownloadUser(beatmap.Creator, maxAttempts: 3);
            Task<OsuScore[]> bestPlayDownloader = OsuNetwork.DownloadOsuBeatmapScores(beatmap, gameMode, 3, 2);

            OsuBoundUserDB bound = await OsuDB.GetBoundUserBy_DiscordID(Context.Message.Author.ID);
            Task<OsuScore> boundBestScoreDownloader = OsuNetwork.DownloadBeatmapBest(beatmap, gameMode, bound.UserID, 3);

            EmbedBuilder eb = beatmap.ToEmbedBuilder(gameMode, creator);

            OsuScore[] bestPlays = await bestPlayDownloader;

            await msg.EditAsync("Fetching best plays...", null);

            if (bestPlays.Length > 0)
            {
                EmbedFieldBuilder rankingsField = new EmbedFieldBuilder();

                rankingsField.Name = $"{OsuGameModesConverter.ToEmoji(gameMode)}";
                rankingsField.Value = "report to LtLi0n";

                OsuUser[] bestPlayUsers = new OsuUser[bestPlays.Length];

                int longestName = int.MinValue;

                for (int i = 0; i < bestPlays.Length; i++)
                {
                    bestPlayUsers[i] = await OsuNetwork.DownloadUser(bestPlays[i].Username);

                    if (bestPlayUsers[i].Username.Length > longestName) longestName = bestPlayUsers[i].Username.Length;
                }

                for (int i = 0; i < bestPlayUsers.Length; i++)
                {
                    string toAdd = ToScoreString(bestPlays[i], bestPlayUsers[i].Country, gameMode, i + 1, longestName);

                    if (i == 0) rankingsField.Value = toAdd;
                    else rankingsField.Value += toAdd;
                }

                //delete after
                string testF = string.Format("{0}", longestName != 0 ? bestPlays[0].Username + new string(new char[longestName - bestPlays[0].Username.Length].Select(X => X = ' ').ToArray()) : bestPlays[0].Username);

                rankingsField.IsInline = true;

                eb.AddField(rankingsField);

                if (bound != null)
                {
                    try
                    {
                        OsuScore boundBestScore = await boundBestScoreDownloader;

                        if (boundBestScore != null)
                        {
                            eb.AddField(x =>
                            {
                                x.Name = $"Your best";
                                x.Value = ToScoreString(boundBestScore, bound.Country, gameMode, nameFormat: '\u200b');
                                x.IsInline = true;
                            });
                        }
                    }
                    catch (Exception e) { }

                }
            }
            else
            {
                string[] lines_f1 = eb.Fields[0].Value.ToString().Split('\n');
                lines_f1[0] += "\t\t\u200b";

                string convertBack = "";
                Tool.ForEach(lines_f1, x => convertBack += (x + '\n'));

                eb.Fields[0].Value = convertBack;

                eb.Fields[1].Value = '\u200b';
            }

            string additional = string.Empty;

            if (action != null) additional = action.ToLower() == "json" ? $"```json\n{JValue.Parse(JsonConvert.SerializeObject(beatmap)).ToString(Formatting.Indented)}```" : string.Empty;

            await msg.EditAsync((additional), embed: eb.Build());
        }

        private static string ToScoreString(OsuScore score, string flag, OsuGameModes gameMode, int? rank = null, int? maxLength = null, char nameFormat = '`') =>
            $"{string.Format("{0}", rank.HasValue ? $"**`#{rank}`** " : string.Empty)}:flag_{flag.ToLower()}: **{nameFormat}{string.Format("{0}", maxLength.HasValue ? score.Username + new string(new char[maxLength.Value - score.Username.Length].Select(c => c = ' ').Append('\u200b').ToArray()) : score.Username)}{nameFormat}**: {score.Score.ToString("#,#", CultureInfo.InvariantCulture)} • **{string.Format("{0:0.##}", score.Accuracy * 100)}%** • {score.ScoreHits.MaxCombo}x • {CustomEmoji.Osu.Rank.FromRank(score.Rank)}{string.Format("{0}", score.Mods.ToLongName() != "NoMod" ? $" • **{score.Mods.ToLongName()}**" : string.Empty)}\n";

        private static string ToScoreString(OsuUserRecent score, string flag, OsuGameModes gameMode, string username, int? rank = null, int? maxLength = null, char nameFormat = '`') =>
            $"{string.Format("{0}", rank.HasValue ? $"**`#{rank}`** " : string.Empty)}:flag_{flag.ToLower()}: **{nameFormat}{string.Format("{0}", maxLength.HasValue ? username + new string(new char[maxLength.Value - username.Length].Select(c => c = ' ').Append('\u200b').ToArray()) : username)}{nameFormat}**: {score.Score.ToString("#,#", CultureInfo.InvariantCulture)} • **{string.Format("{0:0.##}", score.Acccuracy * 100)}%** • {score.ScoreHits.MaxCombo}x • {CustomEmoji.Osu.Rank.FromRank(score.Rank)}{string.Format("{0}", score.Mods.ToLongName() != "NoMod" ? $" • **{score.Mods.ToLongName()}**" : string.Empty)}\n";

    }
}
