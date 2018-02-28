using MrServer.Network;
using System;
using MrServer.Additionals.Design;
using System.Threading.Tasks;
using MrServer.Essentials.Handling;
using MrServer.Bot.Client;
using MrServer.Additionals.Storing;
using MrServer.SQL;
using System.Timers;
using MrServer.SQL.Osu;

namespace MrServer
{
    class Program
    {
        public NetworkHandler NetworkHandler { get; set; }
        public MainHandler mHandler;
        public DiscordClient DiscordClient { get; set; }
        public DataBases DataBases { get; private set; }

        public Timer timer_1min;

        public static Program Entry;

        static void Main(string[] args) => (Entry = new Program()).Start().GetAwaiter().GetResult();

        public async Task Start()
        {
            DataBases = new DataBases();
            await DataBases.Open();

            Console.WriteLine("Starting up...");

            await Keys.LoadAPIKeys();

            mHandler = new MainHandler();
            mHandler.Start();

            NetworkHandler = new NetworkHandler(1994);
            NetworkHandler.Start();

            DiscordClient = new DiscordClient(NetworkHandler);

            Console.WriteLine($"{DesignFunctions.Time.GetTime()} Server started [ {NetworkHandler.tcpListener.LocalEndpoint.ToString()} ]");

            await Task.Delay(-1);
        }
    }
}
