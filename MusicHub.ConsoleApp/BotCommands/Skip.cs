using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicHub.ConsoleApp.BotCommands
{
    public class Skip : BaseCommand
    {
        public override string Command
        {
            get { return "skip"; }
        }

        private readonly IJukebox _jukebox;
        private readonly IUserRepository _userRepository;

        public Skip(IJukebox jukebox, IUserRepository userRepository)
        {
            _jukebox = jukebox;
            _userRepository = userRepository;
        }

        public override void ExecuteCommand(IrcDotNet.IrcClient client, IrcDotNet.IIrcMessageSource source, IList<IrcDotNet.IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            var currentSong = _jukebox.CurrentSong;
            var currentUser = _userRepository.EnsureUser(source.Name, null);

            if (currentSong.UserId != currentUser.Id)
            {
                foreach (var target in targets)
                    client.LocalUser.SendMessage(target, "You can only skip your songs");

                return;
            }

            this._jukebox.SkipTrack();
        }
    }
}
