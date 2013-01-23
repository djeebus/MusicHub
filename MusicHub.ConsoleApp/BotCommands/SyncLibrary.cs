using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicHub.ConsoleApp.BotCommands
{
    public class SyncLibrary : BaseCommand
    {
        private readonly IUserRepository _userRepository;
        private readonly IJukebox _jukebox;

        public override string Command
        {
            get { return "sync-library"; }
        }

        public SyncLibrary(IUserRepository userRepository, IJukebox jukebox)
        {
            this._userRepository = userRepository;
            this._jukebox = jukebox;
        }

        public override void ExecuteCommand(IrcDotNet.IrcClient client, IrcDotNet.IIrcMessageSource source, IList<IrcDotNet.IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            var user = _userRepository.EnsureUser(source.Name, source.Name);

            parameters = NormalizeParameters(parameters);

            if (parameters.Count == 0)
            {
                InvalidSyncLibraryCommand(client, targets, "Missing library id");
                return;
            }

            if (parameters.Count > 1)
            {
                InvalidSyncLibraryCommand(client, targets, "Too many commands");
                return;
            }

            _jukebox.UpdateLibrary(parameters[0]);

            client.LocalUser.SendMessage(targets, "Library sync has been queued");
        }
    }
}