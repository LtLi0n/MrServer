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

            OsuGameModes gameMode = boundUser != null ? boundUser.MainMode : OsuGameModes.STD;

            SocketUserMessage msg = await Context.Channel.SendMessageAsync("Fetching data...", attachID: true);

            OsuUser osuUser = await OsuNetwork.DownloadUser(userName, gameMode, maxAttempts: 2);

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
                        string emoji = CustomEmoji.Osu.Gamemode.GetGamemodeEmoji(userGameModes[i]).ToString();

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
                    OsuUser osuUser = await OsuNetwork.DownloadUser(boundUser.UserID, gameMode);

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
        public async Task OsuBindUser([Remainder]string input)
        {
            OsuBoundUserDB bound = await OsuDB.GetBoundUserBy_DiscordID(Context.Message.Author.ID);

            string[] args = input.Split(' ');

            if (args[0].ToLower() == "mainmode")
            {
                if (bound == null) await Context.Channel.SendMessageAsync("You need to bind your osu user first. `OsuBind [username]`");
                else
                {
                    if (args.Length == 1) await Context.Channel.SendMessageAsync("You need to specify a Game Mode type. It defaults to `standard`.");
                    else
                    {
                        OsuGameModes gameMode = OsuGameModesConverter.FromOfficialName(input.Substring(args[0].Length + 1), true);
                        bound.MainMode = gameMode;
                        await OsuDB.UpdateBoundOsuUser(bound);
                        await Context.Channel.SendMessageAsync($"Main mode has been successfully changed to **{OsuGameModesConverter.GameModeName(gameMode)}**!");
                    }
                }
            }
            else
            {
                OsuUser osuUser = await OsuNetwork.DownloadUser(input, OsuGameModes.STD);

                if (bound == null)
                {
                    await OsuDB.RegisterBoundOsuUser(osuUser, Context.Message.Author.ID);

                    await Context.Channel.SendMessageAsync($"Binded {input} to `{OsuSQL.table_name}`");
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"{input} is already binded.");
                }
            }
        }

        [Command("OsuUnBind")]
        public async Task OsuUnbindUser()
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

            OsuGameModes gameMode = OsuGameModes.STD;

            if (string.IsNullOrEmpty(target))
            {
                OsuBoundUserDB bound = await OsuDB.GetBoundUserBy_DiscordID(Context.Message.Author.ID);

                if (bound != null)
                {
                    targetID = bound.UserID;
                    country = bound.Country;
                    username = bound.UserName;
                    gameMode = bound.MainMode;
                }
                else
                {
                    await msg.EditAsync($"You were not found in the database.\n" +
                        $"To use this command without parameters, proceed to bind your profile with `$osubind [username]`", null);
                }
            }
            else
            {
                if (target.Where(x => x < '0' || x > '9').Count() > 0)
                {
                    //Not ID
                }
                else targetID = ulong.Parse(target);
            }

            OsuUserRecent recent = await OsuNetwork.DownloadUserRecent(targetID.Value, gameMode);

            OsuBeatmap beatmap = await OsuNetwork.DownloadBeatmap(recent.BeatmapID, gameMode);

            if (beatmap == null) await msg.EditAsync("", null);
            else
            {
                OsuUser beatmapCreator = await OsuNetwork.DownloadUser(beatmap.Creator, gameMode);

                EmbedBuilder eb = beatmap.ToEmbedBuilder(gameMode, beatmapCreator, true);

                eb.Fields.Insert(0, new EmbedFieldBuilder
                {
                    Name = $"Recent {CustomEmoji.Osu.Gamemode.GetGamemodeEmoji(gameMode)}",
                    Value = recent.ToScoreString(country, gameMode, username, nameFormat: '\u200b')
                });

                await msg.EditAsync("", eb.Build());
            }
        }

        [Command("BeatmapPack")]
        [Hidden]
        public async Task GetBeatmapPack(string ID)
        {
            SocketUserMessage msg = await Context.Channel.SendMessageAsync("Fetching data...", attachID: true);

            IEnumerable<OsuBeatmap> beatmapPack = await OsuNetwork.DownloadBeatmapPack(int.Parse(ID), logger: msg);

            beatmapPack = beatmapPack.OrderBy(x => x.Difficulty.Rating).OrderBy(x => x.GameMode);

            OsuBeatmap packRef = beatmapPack.First();

            OsuUser creator = await OsuNetwork.DownloadUser(packRef.Creator, OsuGameModes.STD, tolerateNull: true, maxAttempts: 3);

            EmbedBuilder eb = new EmbedBuilder();

            eb.WithAuthor(x =>
            {
                x.Name = packRef.Title;
                x.Url = $"https://osu.ppy.sh/s/{packRef.BeatmapSetID}";
                x.IconUrl = "https://cdn.discordapp.com/attachments/420948614966411299/421301562032390164/beatmapPackLogo.png";
            });

            eb.Thumbnail = $"https://b.ppy.sh/thumb/{packRef.BeatmapSetID}l.jpg";

            eb.Description = "";

            if (creator != null) eb.Description += $"Created By: [{creator.Username}](https://osu.ppy.sh/u/{creator.UserID})\n";

            eb.Description += $"📥 **[Download](https://osu.ppy.sh/d/{packRef.BeatmapSetID}n)**";
            eb.Color = Color.FromArgb(28, 164, 185);

            eb.Footer = packRef.GetFooter(creator);

            eb.AddField(x =>
            {
                x.Name = $"{packRef.Length} ⏱ {packRef.FavouriteCount} ❤️";
                x.Value = $"BPM: **{string.Format("{0:0.##}", packRef.BPM)}**";
            });

            //Display beatmaps
            {
                OsuGameModes previousMode = OsuGameModes.None;

                void addBeatmapField(OsuBeatmap beatmap, bool includeName)
                {
                    eb.AddField(x =>
                    {
                        x.Name = includeName ? $"{CustomEmoji.Osu.Gamemode.GetGamemodeEmoji(beatmap.GameMode)} {OsuGameModesConverter.GameModeName(beatmap.GameMode)}" : "\u200b";
                        x.Value = "empty";

                        x.IsInline = true;
                    });
                }

                foreach (OsuBeatmap beatmap in beatmapPack)
                {
                    bool newField = false;

                    if (previousMode != beatmap.GameMode)
                    {
                        previousMode = beatmap.GameMode;

                        addBeatmapField(beatmap, true);
                        newField = true;
                    }

                    string beatmapInfo = $"{CustomEmoji.Osu.Difficulty.GetDifficultyEmoji(beatmap.Difficulty.Rating, beatmap.GameMode)} **[{beatmap.Version}](https://osu.ppy.sh/b/{beatmap.BeatmapID})** - *{string.Format("{0:0.##}", beatmap.Difficulty.Rating)}★*\n";

                    bool overLimit = eb.Fields[eb.Fields.Count - 1].Value.ToString().Length + beatmapInfo.Length > 1024;

                    if (overLimit)
                    {
                        addBeatmapField(beatmap, false);
                        newField = true;
                    }

                    if (newField) eb.Fields[eb.Fields.Count - 1].Value = beatmapInfo;
                    else eb.Fields[eb.Fields.Count - 1].Value += beatmapInfo;
                }
            }

            //Insert a zero width space char to make a new line or remove useless \n
            for (int i = 1; i < eb.Fields.Count; i++)
            {
                string efbStr = eb.Fields[i].Value.ToString();

                if (i < 3 && eb.Fields.Count > 3) eb.Fields[i].Value = efbStr + '\u200b';
                else eb.Fields[i].Value = efbStr.Remove(efbStr.Length - 1, 1);
            }

            await msg.EditAsync($"showing {beatmapPack.Count()} beatmaps", eb.Build());
        }

        [Command("Beatmap")]
        [Hidden]
        public async Task GetBeatmap(string ID, string mode = null, string action = null)
        {
            SocketUserMessage msg = await Context.Channel.SendMessageAsync("Fetching data...", attachID: true);

            OsuGameModes gameMode = (string.IsNullOrEmpty(mode) ? OsuGameModes.None : OsuGameModesConverter.FromOfficialNumeration(byte.Parse(mode)));

            OsuBeatmap beatmap = await OsuNetwork.DownloadBeatmap(int.Parse(ID), gameMode, msg);
            gameMode = beatmap.GameMode;

            OsuUser creator = await OsuNetwork.DownloadUser(beatmap.Creator, OsuGameModes.STD, tolerateNull: true, maxAttempts: 3);
            Task<OsuScore[]> bestPlayDownloader = OsuNetwork.DownloadBeatmapBest(beatmap, gameMode, scoreCount: 3, logger: msg, tolerateNull: true, maxAttempts: 2);

            OsuBoundUserDB bound = await OsuDB.GetBoundUserBy_DiscordID(Context.Message.Author.ID);
            Task<OsuScore[]> boundBestScoreDownloader = OsuNetwork.DownloadBeatmapBest(beatmap, gameMode, user: bound.UserID, logger: msg, tolerateNull: true, maxAttempts: 3);

            EmbedBuilder eb = beatmap.ToEmbedBuilder(gameMode, creator);

            OsuScore[] bestPlays = await bestPlayDownloader;

            await msg.EditAsync("Fetching best plays...", null);

            if (bestPlays[0] != null)
            {
                EmbedFieldBuilder rankingsField = new EmbedFieldBuilder
                {
                    Name = $"{CustomEmoji.Osu.Gamemode.GetGamemodeEmoji(beatmap.GameMode)}",
                    Value = "report to LtLi0n"
                };

                OsuUser[] bestPlayUsers = new OsuUser[bestPlays.Length];

                int longestName = int.MinValue;

                for (int i = 0; i < bestPlays.Length; i++)
                {
                    bestPlayUsers[i] = await OsuNetwork.DownloadUser(bestPlays[i].Username, beatmap.GameMode, logger: msg, maxAttempts: 2);

                    if (bestPlayUsers[i].Username.Length > longestName) longestName = bestPlayUsers[i].Username.Length;
                }

                for (int i = 0; i < bestPlayUsers.Length; i++)
                {
                    string toAdd = bestPlays[i].ToScoreString(bestPlayUsers[i].Country, gameMode, i + 1, nameFormat: '\u200b');

                    if (i == 0) rankingsField.Value = toAdd;
                    else rankingsField.Value += toAdd;
                }

                rankingsField.IsInline = true;

                eb.AddField(rankingsField);

                if (bound != null)
                {
                    try
                    {
                        OsuScore boundBestScore = (await boundBestScoreDownloader)[0];

                        if (boundBestScore != null)
                        {
                            eb.AddField(x =>
                            {
                                x.Name = $"Your best";
                                x.Value = boundBestScore.ToScoreString(bound.Country, gameMode, includeReplay: boundBestScore.HasReplay, nameFormat: '\u200b');
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
    }
}
