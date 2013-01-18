using MusicHub.ConsoleApp.Irc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicHub.ConsoleApp.BotCommands
{
    public class Stop : BaseCommand
    {
        public override string Command
        {
            get { return "stop"; }
        }

        private readonly IJukebox _jukebox;

        public Stop(IJukebox jukebox)
        {
            _jukebox = jukebox;
        }

        public override void ExecuteCommand(IrcDotNet.IrcClient client, IrcDotNet.IIrcMessageSource source, IList<IrcDotNet.IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            //client.LocalUser.SendMessage(replyTarget, "Music has been stopped");
            IrcHelper.SayInChannels(client, string.Format("{0} has stopped the music", source.Name));

            this._jukebox.Stop();

        }
    }
}
