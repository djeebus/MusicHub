using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicHub.ConsoleApp.BotCommands
{
    public class DeleteLibrary : BaseCommand
    {
        public override string Command
        {
            get { return "delete-library"; }
        }

        private readonly IUserRepository _userRepository;
        private readonly IJukebox _jukebox;

        public DeleteLibrary(IUserRepository userRepository, IJukebox jukebox)
        {
            _jukebox = jukebox;
            _userRepository = userRepository;
        }

        public override void ExecuteCommand(IrcDotNet.IrcClient client, IrcDotNet.IIrcMessageSource source, IList<IrcDotNet.IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            var user = _userRepository.EnsureUser(source.Name, source.Name);

            parameters = NormalizeParameters(parameters);

            if (parameters.Count == 0)
            {
                InvalidDeleteLibraryCommand(client, targets, "Missing library id");
                return;
            }

            if (parameters.Count > 1)
            {
                InvalidDeleteLibraryCommand(client, targets, "Too many commands");
                return;
            }

            var libraryId = parameters[0];
            this._jukebox.DeleteLibrary(libraryId);

            client.LocalUser.SendMessage(targets, "Library has been deleted");
        }

        private void InvalidDeleteLibraryCommand(IrcDotNet.IrcClient client, IList<IrcDotNet.IIrcMessageTarget> replyTarget, string commandError)
        {
            client.LocalUser.SendMessage(replyTarget, string.Format("Invalid .delete-command: {0}", commandError));
            client.LocalUser.SendMessage(replyTarget, "Usage: .delete-library library-guid");
        }
    }
}
