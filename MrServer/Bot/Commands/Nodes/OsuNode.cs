using MrServer.SQL.Osu;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using MrServer.Bot.Commands.Permissions;
using MrServerPackets.Discord.Models.Guilds;
using MrServerPackets.ApiStructures.Osu.Database;
using MrServerPackets.ApiStructures.Osu;
using MrServerPackets.Discord.Entities;
using System.Globalization;
using System.Drawing;
using MrServer.Bot.Client;
using System.Threading;
using MrServer.Bot.Commands.Attributes;
using System.Reflection;
using MrServer.Bot.Commands.Attributes.Permissions;
using MrServer.Bot.Models;
using MrServer.Network.Osu;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MrServerPackets.Discord.Models;

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

            if (boundUser != null) userName = boundUser.UserName;

            if (Context.Message.Mentions.Length == 1 && boundUser == null)
            {
                await Context.Channel.SendMessageAsync("Mentioned user is not binded.");
                return;
            }

            SocketUserMessage msg = await Context.Channel.SendMessageAsync("Fetching data...", attachID: true);

            OsuUser osuUser = await Network.Osu.OsuNetwork.DownloadOsuUser(userName, maxAttempts: 2);

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
                        string emoji = string.Empty;

                        switch (userGameModes[i])
                        {
                            case OsuGameModes.STD: emoji = "<:STD:415867031087480833>"; break;
                            case OsuGameModes.Taiko: emoji = "<:Taiko:415867092311605252>"; break;
                            case OsuGameModes.CtB: emoji = "<:CtB:415867131851309056>"; break;
                            case OsuGameModes.Mania: emoji = "<:Mania:415867163279228938>"; break;
                        }


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
                    OsuUser osuUser = await Network.Osu.OsuNetwork.DownloadOsuUser(boundUser.UserID, OsuGameModesConverter.ToOfficialNumeration(gameMode));

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
            OsuUser osuUser = await Network.Osu.OsuNetwork.DownloadOsuUser(userName);

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

        [Command("Beatmap")]
        [RequireOwner]
        [Hidden]
        public async Task GetBeatmap(string ID, string action = null)
        {
            SocketUserMessage msg = await Context.Channel.SendMessageAsync("Fetching data...", attachID: true);

            OsuBeatmap beatmap = await OsuNetwork.DownloadOsuBeatmap(int.Parse(ID), false);

            OsuUser creator = await OsuNetwork.DownloadOsuUser(beatmap.Creator, maxAttempts: 3);
            Task<OsuScore[]> bestPlayDownloader = OsuNetwork.DownloadOsuBeatmapScores(beatmap, 3);

            EmbedBuilder eb = new EmbedBuilder();

            eb.Thumbnail = $"https://b.ppy.sh/thumb/{beatmap.BeatmapSetID}l.jpg";
            eb.Author = new EmbedAuthorBuilder()
            {
                IconUrl = GetDifficultyIconURL(beatmap.Difficulty.Rating, beatmap.GameMode),
                Name = $"{beatmap.Title} ({beatmap.Version})",
                Url = $"https://osu.ppy.sh/b/{beatmap.BeatmapID}&m={(byte)beatmap.GameMode}"
            };

            eb.Color = Color.FromArgb(28, 164, 185);

            eb.Description = 
                $"Created by: **[{creator.Username}](https://osu.ppy.sh/u/{creator.UserID})**\n" +
                $"📥 **[Download](https://osu.ppy.sh/d/{beatmap.BeatmapSetID}n)** 🖼 [With Video](https://osu.ppy.sh/d/{beatmap.BeatmapSetID})";

            eb.AddField(x =>
            {
                x.Name = $"{string.Format("{0:0.##}", beatmap.Difficulty.Rating)} ⭐ {beatmap.Length} ⏱";
                x.Value =
                $"AR: **{beatmap.Difficulty.Approach}** • CS: **{beatmap.Difficulty.Size}** • OD: **{beatmap.Difficulty.Overall}** • Drain: **{beatmap.Difficulty.Drain}**\n" +
                $"BPM: **{beatmap.BPM}**";
                x.IsInline = true;
            });

            eb.AddField(x =>
            {
                x.Name = $"{beatmap.FavouriteCount} ❤️";
                x.Value =
                $"• Plays: **{beatmap.PlayCount}**\n" +
                $"• Passes: **{beatmap.PassCount}**";

                x.IsInline = true;
            });

            OsuScore[] bestPlays = await bestPlayDownloader;

            await msg.EditAsync("Fetching best plays...", null);

            if (bestPlays != null)
            {
                EmbedFieldBuilder rankingsField = new EmbedFieldBuilder();

                rankingsField.Name = "🏆";
                rankingsField.Value = "report to LtLi0n";

                for (int i = 0; i < bestPlays.Length; i++)
                {
                    OsuUser bestPlayUser = await OsuNetwork.DownloadOsuUser(bestPlays[i].Username);

                    string toAdd = $"`#{i + 1}` :flag_{bestPlayUser.Country.ToLower()}: {bestPlays[i].Username} • Score: **{bestPlays[i].Score.ToString("#,#", CultureInfo.InvariantCulture)}**\n";

                    if (i == 0) rankingsField.Value = toAdd;
                    else rankingsField.Value += toAdd;
                }

                rankingsField.IsInline = true;

                eb.AddField(rankingsField);
            }

            eb.Footer = new EmbedFooterBuilder()
            {
                Text = $"Ranked: {beatmap.ApprovalDate.ToShortDateString()} • Last Update: {beatmap.LastUpdate.ToShortDateString()}",
                IconUrl = creator != null ? creator.AvatarURL : string.Empty
            };

            string additional = string.Empty;

            if (action != null) additional = action.ToLower() == "json" ? $"```json\n{JValue.Parse(JsonConvert.SerializeObject(beatmap)).ToString(Formatting.Indented)}```" : string.Empty;

            try
            {
                Embed embed = eb.Build();

                await msg.EditAsync((additional), embed: embed);
            }
            catch(Exception e) { Console.WriteLine(e); }
            
        }

        private string GetDifficultyIconURL(float rating, OsuGameModes gameMode)
        {
            string diffIcon = string.Empty;

            if (rating < 1.50) diffIcon = "easy";
            else if (rating < 2.25) diffIcon = "normal";
            else if (rating < 3.75) diffIcon = "hard";
            else if (rating < 5.25) diffIcon = "insane";
            else if (rating < 6.75) diffIcon = "expert";
            else diffIcon = "expert";

            switch (gameMode)
            {
                case OsuGameModes.Taiko: diffIcon += "-t"; break;
                case OsuGameModes.CtB: diffIcon += "-f"; break;
                case OsuGameModes.Mania: diffIcon += "-m"; break;
            }

            return $"https://s.ppy.sh/images/{diffIcon}.png";
        }
    }
}
