using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MrServer.Bot.Models.Artificial
{
    public class InteractiveUser
    {
        public string Location { get; set; }
        public ulong ChannelID { get; }
        public ulong UserID { get; }

        private int _idleMax { get; }
        private int _idle;

        public bool Expired => _idle == _idleMax;
        public bool DeleteReady { get; private set; }

        private bool _forcedExit;

        private SocketUserMessage lastBotMsg;

        public List<string> Inputs { get; set; }

        public InteractiveUser(ulong channelID, ulong userID)
        {
            Location = "o";
            ChannelID = channelID;
            UserID = userID;

            _idleMax = 20;

            Inputs = new List<string>();

            InternalClock();
        }

        public async Task Exit()
        {
            _forcedExit = true;

            if (lastBotMsg != null) await lastBotMsg.DeleteAsync();
        }

        public async Task Refresh(SocketUserMessage botMsg)
        {
            if(lastBotMsg != null) await lastBotMsg.DeleteAsync();
            lastBotMsg = botMsg;

            _idle = 0;
        }

        private async Task InternalClock()
        {
            for(; _idle < _idleMax && !_forcedExit; _idle++) await Task.Delay(1000);

            if(!_forcedExit)
            {
                if (lastBotMsg != null) await lastBotMsg.DeleteAsync();
                lastBotMsg = await lastBotMsg.Channel.SendMessageAsync("Interactive message has been closed due to inactivity.");

                await Task.Delay(3000);

                await lastBotMsg.DeleteAsync();
            }

            DeleteReady = true;
            Console.WriteLine("IsDeleteReady");
        }
    }
}
