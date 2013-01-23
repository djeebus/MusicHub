using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicHub.ConsoleApp.BotCommands
{
    public class MostHatedUsers : BaseCommand
    {
        public override string Command
        {
            get { return "most-hated-users"; }
        }

        private readonly IAffinityTracker _affinityTracker;
        public MostHatedUsers(IAffinityTracker affinityTracker)
        {
            _affinityTracker = affinityTracker;
        }

        public override void ExecuteCommand(IrcDotNet.IrcClient client, IrcDotNet.IIrcMessageSource source, IList<IrcDotNet.IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            var users = _affinityTracker.GetMostHatedUsers();

            Action<string> say = msg => client.LocalUser.SendMessage(targets, msg);

            say(string.Format("The {0} most hated users are:", users.Length));
            for (int x = 0; x < users.Length; x++)
                say(string.Format("{0}) {1}", x + 1, users[x]));
        }
    }
}
