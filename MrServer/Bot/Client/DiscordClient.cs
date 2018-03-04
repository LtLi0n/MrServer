using MrServer.Bot.Commands;
using MrServer.Network;
using MrServer.Network.EventArgModels;
using MrServerPackets.Discord.Models;
using MrServerPackets.Discord.Packets;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MrServerPackets;
using MrServerPackets.Headers;
using MrServerPackets.Discord.Models.Guilds;
using MrServer.Bot.Models;
using System.Linq;
using MrServer.Network.Osu;

namespace MrServer.Bot.Client
{
    public class DiscordClient
    {
        public const ulong ID_ME = 174230278611533824;
        public const ulong ID_BOT = 408965188931551233;

        public NetworkHandler network;
        public CommandService cService;

        public string PREFIX { get; private set; }

        public List<SocketMessage> Messages { get { return network.DiscordMessages; } set { network.DiscordMessages = value; } }
        public List<SocketUserMessage> TrackedMessages;

        private bool waitingForBotMessage => waitingForBotMessages > 0;
        private int waitingForBotMessages = 0;

        private OsuNetwork osu;

        public DiscordClient(NetworkHandler network)
        {
            TrackedMessages = new List<SocketUserMessage>();

            PREFIX = "$";
            cService = new CommandService(this);
            //cService.WaitUntilLoaded().GetAwaiter().GetResult();

            this.network = network;

            network.DiscordMessageReceived += NetworkHandler_DiscordMessageReceived;

            osu = new OsuNetwork(this);
        }

        public async Task<SocketUserMessage> GetBotMessageAsync(string expectedID)
        {
            waitingForBotMessages++;

            while (true)
            {
                if(TrackedMessages.Count > 0)
                {
                    for(int i = 0; i < TrackedMessages.Count; i++)
                    {
                        if (TrackedMessages[i].UniqueID == expectedID)
                        {
                            SocketUserMessage toReturn = TrackedMessages[i];

                            TrackedMessages.Remove(toReturn);

                            return toReturn;
                        }
                    }

                }

                await Task.Delay(1);
            }

            waitingForBotMessages--;
        }

        private async void NetworkHandler_DiscordMessageReceived(object sender, DiscordMessageReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Message.UniqueID)) TrackedMessages.Add(e.Message);

            if (e.Message.Author.IsBot) return;

            if (!string.IsNullOrEmpty(e.Message.Content))
            {
                //Trigger command response
                if (e.Message.Content.Substring(0, PREFIX.Length) == PREFIX)
                {
                    string cmd = e.Message.Content.Split(' ')[0].Remove(0, PREFIX.Length);

                    string input = "";

                    if (e.Message.Content.Length > cmd.Length + 1)
                    {
                        input = e.Message.Content.Substring(cmd.Length + 2, e.Message.Content.Length - cmd.Length - 2);
                    }

                    try
                    {
                        bool success = await cService.ExecuteAsync(e.Message.Content.Split(' ')[0].Remove(0, PREFIX.Length), input, e.Message);
                    }
                    catch (Exception ee)
                    {
                        Console.WriteLine(ee);
                    }
                }
            }

            Messages.Remove(e.Message);
        }

        public async Task SendMessageAsync(string content, ulong channelID, ulong guildID, Embed embed = null)
        {
            PacketWriter pw = new PacketWriter(Header.Discord);
            pw.WriteHeader(DiscordHeader.SendMessage);

            pw.WriteJSON(new GuildMessagePacket(
                new GuildChannel(
                    new Guild() { ID = guildID },
                    new Channel() { ID = channelID }),
                new MessagePacket(new Message()
                {
                    Content = content,
                    Embed = embed
                })), true);

            await network.Send<DataTCP>(pw.GetBytes(), network.DiscordTCP);
        }
    }
}
