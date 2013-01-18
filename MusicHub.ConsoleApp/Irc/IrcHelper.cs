using IrcDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicHub.ConsoleApp.Irc
{
    public static class IrcHelper
    {
        public static void SayInChannels(IrcClient client, string msg)
        {
            foreach (var channel in client.Channels)
            {
                client.LocalUser.SendMessage(channel, msg);
            }
        }
    }
}
