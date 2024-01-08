using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HolyHomie.Models
{
    public class MessagesAwaitResults
    {
        public ulong ID { get; set; }
        public IUserMessage Message { get; set; }
        public SocketUser? User { get; set; }
        public List<Tracks> tracks { get; set; }
    }
}
