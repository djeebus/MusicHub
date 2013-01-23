using MusicHub.ConsoleApp.Irc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicHub.ConsoleApp.BotCommands
{
    public class Love : BaseCommand
    {
        public override string Command
        {
            get { return "love"; }
        }

        private readonly IJukebox _jukebox;
        private readonly IUserRepository _userRepository;

        public Love(IJukebox jukebox, IUserRepository userRepository)
        {
            _jukebox = jukebox;
            _userRepository = userRepository;
        }

        public override void ExecuteCommand(IrcDotNet.IrcClient client, IrcDotNet.IIrcMessageSource source, IList<IrcDotNet.IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            IrcHelper.SayInChannels(client, string.Format("{0} loves this song", source.Name));

            var user = this._userRepository.EnsureUser(source.Name, source.Name);

            try
            {
                _jukebox.Love(user.Id);
            }
            catch
            {
                client.LocalUser.SendMessage(targets, "There is no song playing; please do not love w/o a partner in public!");
                return;
            }
        }
    }
}
