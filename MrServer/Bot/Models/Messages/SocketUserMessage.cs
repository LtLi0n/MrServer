using MrServer.Bot.Client;
using MrServer.Network;
using MrServerPackets;
using MrServerPackets.Discord.Models;
using MrServerPackets.Discord.Packets;
using MrServerPackets.Headers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MrServer.Bot.Models
{
    public class SocketUserMessage : SocketMessage
    {
        public SocketUser Author { get; set; }

        public SocketUserMessage(NetworkHandler network, SocketChannel channel, Message message, SocketUser author) : base(network, channel, message)
        {
            Author = author;
        }

        public async Task<SocketUserMessage> EditAsync(string content, Embed embed)
        {
            Content = content;
            Embed = embed;

            //Send to DiscordClient
            {
                PacketWriter pw = new PacketWriter(Header.Discord);
                pw.WriteHeader(DiscordHeader.SendMessage);

                string json = JsonConvert.SerializeObject(ToGuildMessagePacket("EDIT"), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });

                pw.WriteJSON(ToGuildMessagePacket("EDIT"), true);

                await _network.Send<DataTCP>(pw.GetBytes(), _network.DiscordTCP);
            }

            return this;
        }

        public async Task DeleteAsync()
        {
            PacketWriter pw = new PacketWriter(Header.Discord);
            pw.WriteHeader(DiscordHeader.SendMessage);

            pw.WriteJSON(ToGuildMessagePacket("DELETE"), true);

            await _network.Send<DataTCP>(pw.GetBytes(), _network.DiscordTCP);
        }
    }
}
