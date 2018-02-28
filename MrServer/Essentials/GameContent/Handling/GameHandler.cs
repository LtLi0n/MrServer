using MrServer.Essentials.GameContent.Players;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MrServer.Essentials.GameContent.Handling
{
    public class GameHandler
    {
        public List<Player> Players { get; set; }

        Timer UpdateTimer;

        public GameHandler()
        {
            Players = new List<Player>();

            UpdateTimer = new Timer(1 / 60);
            UpdateTimer.Elapsed += new ElapsedEventHandler(Update);
            UpdateTimer.Start();
        }

        //1 tick every 1/60s
        private void Update(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("tick...");
        }

        public async Task Start()
        {
            await Load();
        }

        private async Task Load()
        {

        }
    }
}
