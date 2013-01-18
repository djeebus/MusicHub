using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicHub.ConsoleApp.BotCommands
{
    public class Play : BaseCommand
    {
        public override string Command
        {
            get { return "play"; }
        }

        private readonly IJukebox _jukebox;

        public Play(IJukebox jukebox)
        {
            this._jukebox = jukebox;
        }

        public override void ExecuteCommand(IrcDotNet.IrcClient client, IrcDotNet.IIrcMessageSource source, IList<IrcDotNet.IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            _jukebox.Play();
        }
    }
}
