using MrServer.SQL.Management;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MrServer.Bot.Models.Artificial.Models
{
    public class GuildManagementInteractiveHandler : InteractiveMessageHandler
    {
        private CustomGuildSQL GuildDB => Program.Entry.DataBases.GuildDB;

        public GuildManagementInteractiveHandler() : base()
        {

        }

        protected override async Task Init()
        {
            var methods = new Func<InteractiveEventArgs, Task<(string, InteractiveEventArgs)>>[]
            {
                o, //Entry
                o_prefix, o_prefix_select, o_prefix_select_final //Prefix
            };

            await ConvertToHierarchy(methods);
        }

        [Location("o")]
        private async Task<(string, InteractiveEventArgs)> o(InteractiveEventArgs e)
        {
            e.Displayer = 
                ">Guild management control panel<\n" +
                "• Prefix";

            string input = e.Message.Content.ToLower();

            if (!e.IsSimulated) if (input == "prefix") return ("o->1", e);

            return ("o", e);
        }

        [Location("o->1")]
        private async Task<(string, InteractiveEventArgs)> o_prefix(InteractiveEventArgs e)
        {
            string prefix = await GuildDB.GetCustomPrefix(((e.Channel as SocketGuildChannel).Guild.ID), e.Discord.PREFIX);

            e.Displayer =
                ">Custom prefix selector<\n" +
                "• Change Prefix\n\n" +
                $"Current prefix: `{prefix}`";

            if (!e.IsSimulated)
            {
                string input = e.Message.Content.ToLower();

                if(input == "change prefix") return ("o->1->1", e);
            }

            return ("o->1", e);
        }

        [Location("o->1->1")]
        private Task<(string, InteractiveEventArgs)> o_prefix_select(InteractiveEventArgs e)
        {
            e.Displayer =
                "Select your custom prefix:\n" +
                "*Up to 2 character length*";

            if (!e.IsSimulated)
            {
                Console.WriteLine("test");

                string input = e.Message.Content.ToLower();

                if(!string.IsNullOrEmpty(input))
                {
                    if (input.Length < 3)
                    {
                        e.User.Inputs.Add(input);
                        return Task.FromResult(("o->1->1->1", e));
                    }
                }
            }

            return Task.FromResult(("o->1->1", e));
        }

        [Location("o->1->1->1")]
        private async Task<(string, InteractiveEventArgs)> o_prefix_select_final(InteractiveEventArgs e)
        {
            e.Displayer = 
                ">Prefix Confirmation<\n" +
                "Type your selected prefix to confirm the selection:";

            if (!e.IsSimulated)
            {
                string input = e.Message.Content.ToLower();

                if (!string.IsNullOrEmpty(input))
                {
                    if (e.User.Inputs[0] == input)
                    {
                        await GuildDB.SavePrefix((e.Channel as SocketGuildChannel).Guild.ID, input, e.Discord.PREFIX);

                        e.Displayer = $"Guild prefix successfully changed to `{input}`!";

                        e.User.Inputs.Clear();

                        return ("o", e);
                    }
                }
            }

            return ("o->1->1->1", e);
        }
    }
}
