using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Events
{
    [EventHandler]
    public class CheckCreatedChannel : ApplicationCommandsModule
    {
        [Event(DiscordEvent.ChannelCreated)]
        public async Task CreatedChannel(DiscordClient sender, ChannelCreateEventArgs e)
        {
            while (true)
            {
                await Task.Delay(30000);

                if (e.Channel.Users.Count < 1)
                {
                    await e.Channel.DeleteAsync();

                    return;
                }
            }
        }
    }
}
