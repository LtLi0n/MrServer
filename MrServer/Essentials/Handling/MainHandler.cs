using MrServer.Essentials.GameContent.Handling;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace MrServer.Essentials.Handling
{
    public class MainHandler
    {
        private Thread handlerThread;
        private ulong ticks = 0;

        GameHandler GameHandler { get; set; }

        public MainHandler()
        {
            handlerThread = new Thread(Run);
        }

        public void Start() { handlerThread.Start(); }
        public void Stop() { handlerThread.Join(); }

        private void Run()
        {
            while (true)
            {
                Update();

                Thread.Sleep(1);
            }
        }

        private void Update()
        {
            ticks++;

            if (ticks % ((1000 / 60) * 60) == 0)
            {
                //Console.WriteLine("tick");

                //Update_Players();
            }

            //AUTO-SAVE EVERY 10 MINUTES
            if (ticks % ((1000 / 60) * 60 * 10) == 0)
            {
                //IO.Save(world);
            }
        }
    }
}
