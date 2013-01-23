using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicHub.ConsoleApp.BotCommands
{
    public class MostHatedArtists : BaseCommand
    {
        public override string Command
        {
            get { return "most-hated-artists"; }
        }

        private readonly IAffinityTracker _affinityTracker;
        public MostHatedArtists(IAffinityTracker affinityTracker)
        {
            _affinityTracker = affinityTracker;
        }

        public override void ExecuteCommand(IrcDotNet.IrcClient client, IrcDotNet.IIrcMessageSource source, IList<IrcDotNet.IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            var artists = _affinityTracker.GetMostHatedArtists();

            Action<string> say = msg => client.LocalUser.SendMessage(targets, msg);

            say(string.Format("The {0} most hated artists are:", artists.Length));
            for (int x = 0; x < artists.Length; x++)
                say(string.Format("{0}) {1}", x + 1, artists[x]));
        }
    }
}
