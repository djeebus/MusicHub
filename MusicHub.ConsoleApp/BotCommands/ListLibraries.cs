using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicHub.ConsoleApp.BotCommands
{
    class ListLibraries : BaseCommand
    {
        public override string Command
        {
            get { return "list-libraries"; }
        }

        private readonly IUserRepository _userRepository;
        private readonly IJukebox _jukebox;

        public ListLibraries(IUserRepository userRepository, IJukebox jukebox)
        {
            _jukebox = jukebox;
            _userRepository = userRepository;
        }

        public override void ExecuteCommand(IrcDotNet.IrcClient client, IrcDotNet.IIrcMessageSource source, IList<IrcDotNet.IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            var user = _userRepository.EnsureUser(source.Name, source.Name);

            var libraries = this._jukebox.GetLibrariesForUser(user.Id);

            client.LocalUser.SendMessage(targets, "Your libraries: ");
            foreach (var library in libraries)
            {
                client.LocalUser.SendMessage(targets, string.Format("  {0} (songs: {1}) (id: {2})", library.Name, library.TotalSongs, library.Id));
            }
        }
    }
}
