using MrServer.Network.EventArgModels;
using MrServerPackets;
using MrServerPackets.Headers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Threading.Tasks;
using MrServerPackets.Discord.Models;
using MrServerPackets.Discord.Models.Guilds;
using MrServer.Bot.Models;
using MrServer.Bot.Client;
using System.Net.Http;
using MrServerPackets.ApiStructures.Osu;

namespace MrServer.Network
{
    public interface IDataType { }

    public struct DataTCP : IDataType { }
    public struct DataUDP : IDataType { }

    public class NetworkHandler
    {
        public TcpListener tcpListener;
        public UdpClient udpListener;
        Thread receiveThread;
        Thread receiveThreadUDP;
        bool running = false;

        public TcpClient DiscordTCP;

        public delegate void DiscordMessageReceivedEventHandler(object sender, DiscordMessageReceivedEventArgs e);
        public event DiscordMessageReceivedEventHandler DiscordMessageReceived;
        protected virtual void OnDiscordMessageReceived(DiscordMessageReceivedEventArgs e) => DiscordMessageReceived?.Invoke(this, e);

        public List<SocketMessage> DiscordMessages { get; set; }

        //UDP receive thread needs fixing
        //Currently no use of UDP anyway
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }

        public NetworkHandler(int port)
        {
            IPAddress localAddr = IPAddress.Parse(GetLocalIPAddress());//IPAddress.Parse("192.168.1.170");
            IPEndPoint address = new IPEndPoint(IPAddress.Any, port);

            tcpListener = new TcpListener(address);
            udpListener = new UdpClient(address);

            receiveThread = new Thread(_Start);
            DiscordMessages = new List<SocketMessage>();
        }

        public void Start()
        {
            running = true;
            receiveThread.Start();
        }

        public void Stop() => running = false;

        private void _Start()
        {
            tcpListener.Start();

            while (running)
            {
                TcpClient client = tcpListener.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem((s) => WorkThread(s, client).GetAwaiter().GetResult());
            }

            tcpListener.Stop();
            receiveThread.Join();
            receiveThreadUDP.Join();
        }

        private void UdpThread()
        {
            while (running)
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

                byte[] data = udpListener.Receive(ref sender);

                PacketReader pr = new PacketReader(data);

                Header header = (Header)pr.ReadUInt16();
            }
        }

        //TCP
        private async Task WorkThread(object s, TcpClient client)
        {
            DiscordTCP = client;

            client.NoDelay = true;
            Console.WriteLine($"{client.Client.RemoteEndPoint.ToString()} Connected!");

            try
            {
                while (client.Connected)
                {
                    bool disconnected = false;

                    while (client.Available < 8)
                    {
                        if (!client.Connected)
                        {
                            disconnected = true;
                            break;
                        }
                        await Task.Delay(1);
                    }

                    if (disconnected) break;

                    int length = 0; //Read Length Size
                    {
                        byte[] lengthB = new byte[4];
                        client.GetStream().Read(lengthB, 0, 4);
                        length = BitConverter.ToInt32(lengthB, 0);
                    }

                    PacketReader pr_h1, pr_h2; //Headers
                    PacketReader pr_data = null;
                    {
                        //Header 1
                        byte[] h1 = new byte[2];
                        client.GetStream().Read(h1, 0, 2);
                        pr_h1 = new PacketReader(h1);
                        pr_h1.ReadHeader();

                        //Header 2
                        byte[] h2 = new byte[2];
                        client.GetStream().Read(h2, 0, 2);
                        pr_h2 = new PacketReader(h2);

                        //If there is data, proceed to read it
                        if(length > 0)
                        {
                            while(client.Available < length) await Task.Delay(1);

                            //Data
                            byte[] dataB = new byte[length];
                            client.GetStream().Read(dataB, 0, length);
                            pr_data = new PacketReader(dataB);
                        }
                    }

                    string json = string.Empty;

                    if (length > 0) json = Encoding.Unicode.GetString(pr_data.ReadBytes(length));

                    if(pr_h1.Header == Header.Discord)
                    {
                        var DiscordHeader = pr_h2.ReadSubHeader<DiscordHeader>();

                        if (DiscordHeader == DiscordHeader.ReceiveMessage)
                        {
                            JsonSerializerSettings jsonSettings = new JsonSerializerSettings();
                            jsonSettings.TypeNameHandling = TypeNameHandling.Auto;

                            OnDiscordMessageReceived(new DiscordMessageReceivedEventArgs(JsonConvert.DeserializeObject<UserMessage>(json, jsonSettings), this));
                        }
                    }
                }

                Console.WriteLine("Client disconnected");
            }

            catch (Exception e)
            {
                if (e.GetType() != typeof(SocketException))
                {
                    Console.WriteLine(e.Message);
                }
            }

            if (client.Client.Connected)
            {
                client.Close();
            }
        }

        public Task Send<T>(byte[] data, TcpClient client)
        {
            if (typeof(T).Name == "DataTCP")
            {
                client.Client.BeginSend(data, 0, data.Length, SocketFlags.None, OnSent<T>, client);
            }
            else
            {
                //UDP and whatever else
                throw new NotImplementedException();
            }

            return Task.CompletedTask;
        }

        [Obsolete]
        private void OnSent<T>(IAsyncResult result)
        {

        }

        public static async Task<string> DownloadJSON(string url, int maxAttempts)
        {
            using (HttpClient htppClient = new HttpClient())
            {
                for (int i = 0; i < maxAttempts; i++)
                {
                    try
                    {
                        string json = await htppClient.GetStringAsync(url);

                        return json;
                    }
                    catch (Exception e) { Console.WriteLine(e.Message); }
                }
            }

            return null;
        }
    }
}
