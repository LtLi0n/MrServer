using MrServer.Additionals.Tools;
using MrServer.Bot.Client;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MrServer.Bot.Models.Artificial
{
    public abstract class InteractiveMessageHandler
    {
        protected Dictionary<string, Func<InteractiveEventArgs, Task<(string, InteractiveEventArgs)>>> _hierarchy;
        public List<InteractiveUser> Users { get; protected set; }

        protected InteractiveMessageHandler()
        {
            _hierarchy = new Dictionary<string, Func<InteractiveEventArgs, Task<(string, InteractiveEventArgs)>>>();
            Users = new List<InteractiveUser>();

            Init();
        }

        protected abstract Task Init();

        protected Task ConvertToHierarchy(Func<InteractiveEventArgs, Task<(string, InteractiveEventArgs)>>[] methods)
        {
            foreach(var method in methods)
            {
                Location location = method.Method.GetCustomAttribute<Location>();

                _hierarchy.Add(location.ToString(), method);
            }

            InternalClock();

            return Task.CompletedTask;
        }

        ///<summary>Check if input was update type(exit, back...). If exit was requested, return false to ignore further calls.</summary>
        private async Task<bool> Update(SocketUserMessage msg, InteractiveUser user)
        {
            string input = msg.Content.ToLower();

            if(input == "back")
            {
                MatchCollection linkCollection = Regex.Matches(user.Location, @"->\d+");

                //If there is at least one location linker
                if (linkCollection.Count > 0)
                {
                    string lastLink = linkCollection[linkCollection.Count - 1].Value;

                    user.Location = user.Location.Remove(user.Location.Length - lastLink.Length, lastLink.Length);
                }
            }
            else if(input == "exit")
            {
                await user.Exit();
                Users.Remove(user);
                return false;
            }

            return true;
        }

        private async Task InternalClock()
        {
            while(true)
            {
                Users.RemoveAll(x => x.DeleteReady);

                await Task.Delay(1000);
            }
        }

        public async Task AddUser(SocketUserMessage msg, InteractiveUser user, DiscordClient discord)
        {
            Users.Add(user);
            await HandleInteraction(msg, user, discord);
        }

        public async Task HandleInteraction(SocketUserMessage msg, InteractiveUser user, DiscordClient discord)
        {
            if (!user.DeleteReady)
            {
                if(await Update(msg, user))
                {
                    //Execute code
                    user.Location = (await _hierarchy[user.Location](new InteractiveEventArgs(msg, discord, user))).Item1;
                    //Show next display but hold code
                    string toDisplay = (await _hierarchy[user.Location](new InteractiveEventArgs(msg, discord, user) { IsSimulated = true })).Item2.Displayer;
                    
                    SocketUserMessage botMsg = await msg.Channel.SendMessageAsync(toDisplay, attachID: true);

                    await user.Refresh(botMsg);
                }
            }
        }
    }
}
