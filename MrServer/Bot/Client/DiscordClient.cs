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

namespace MrServer.Bot.Client
{
    public class DiscordClient
    {
        public const ulong ID_ME = 174230278611533824;

        public NetworkHandler network;
        public CommandService cService;

        public string PREFIX { get; private set; }

        public DiscordClient(NetworkHandler NetworkHandler)
        {
            PREFIX = "$";
            cService = new CommandService(this);
            //cService.WaitUntilLoaded().GetAwaiter().GetResult();

            network = NetworkHandler;

            NetworkHandler.DiscordMessageReceived += NetworkHandler_DiscordMessageReceived;
        }

        private async void NetworkHandler_DiscordMessageReceived(object sender, DiscordMessageReceivedEventArgs e)
        {
            if (e.Message. Author.IsBot) return;

            if(!string.IsNullOrEmpty(e.Message.Content))
            {
                SocketUserMessage guildMessage = e.Message as SocketUserMessage;

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
                        await cService.ExecuteAsync(e.Message.Content.Split(' ')[0].Remove(0, PREFIX.Length), input, guildMessage);
                    }
                    catch (Exception ee)
                    {
                        Console.WriteLine(ee);
                    }
                }
            }
        }

        public async Task SendMessageAsync(string content, ulong channelID, ulong guildID, Embed Embed = null)
        {
            PacketWriter pw = new PacketWriter(Header.Discord);
            pw.WriteHeader(DiscordPacketTypeHeader.SendMessage_GuildChannel);

            pw.WriteJSON(new GuildMessagePacket { Content = content, Channel = new GuildChannel(new Channel() { ID = channelID }, new Guild() { ID = guildID}), Embed = Embed });

            await network.Send<DataTCP>(pw.GetBytes(), network.DiscordTCP);
        }

        public async Task SendDMMessageAsync(string Content, SocketUser User, Embed Embed = null)
        {
            PacketWriter pw = new PacketWriter(Header.Discord);
            pw.WriteHeader(DiscordPacketTypeHeader.SendMessage_DM);

            pw.WriteJSON(new SendDMMessagePacket { Content = Content, User = User, Embed = Embed });

            await network.Send<DataTCP>(pw.GetBytes(), network.DiscordTCP);
        }
    }
}
