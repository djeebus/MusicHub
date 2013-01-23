using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicHub.ConsoleApp.BotCommands
{
    public class AddLibrary : BaseCommand
    {
        public override string Command
        {
            get { return "add-library"; }
        }

        private readonly IUserRepository _userRepository;
        private readonly IJukebox _jukebox;

        public AddLibrary(IUserRepository userRepository, IJukebox jukebox)
        {
            this._userRepository = userRepository;
            this._jukebox = jukebox;
        }

        public override void ExecuteCommand(IrcDotNet.IrcClient client, IrcDotNet.IIrcMessageSource source, IList<IrcDotNet.IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            var user = _userRepository.EnsureUser(source.Name, source.Name);

            parameters = NormalizeParameters(parameters);

            if (parameters.Count < 2)
            {
                InvalidAddLibraryCommand(client, targets);
                return;
            }

            var type = parameters[0];
            switch ((type ?? string.Empty).ToLower())
            {
                case "folder":
                    if (parameters.Count != 2)
                    {
                        InvalidAddLibraryCommand(client, targets);
                        return;
                    }

                    var path = parameters[1];
                    _jukebox.CreateLibrary(user.Id, LibraryType.SharedFolder, path, null, null);

                    client.LocalUser.SendMessage(targets, string.Format("'{0}' has been added as a library, now syncing ...", path));
                    break;

                case "google":
                    if (parameters.Count != 3)
                    {
                        InvalidAddLibraryCommand(client, targets);
                        return;
                    }

                    var username = parameters[1];
                    var password = parameters[2];


                    _jukebox.CreateLibrary(user.Id, LibraryType.GoogleMusic, null, username, password);
                    client.LocalUser.SendMessage(targets, string.Format("{0}'s google music library has been added as a library, now syncing ...", username));
                    break;

                default:
                    InvalidAddLibraryCommand(client, targets);
                    return;
            }
        }

        private void InvalidAddLibraryCommand(IrcDotNet.IrcClient client, IList<IrcDotNet.IIrcMessageTarget> replyTarget)
        {
            client.LocalUser.SendMessage(replyTarget, "Invalid command");
            client.LocalUser.SendMessage(replyTarget, ".add-library folder \"path\"");
            client.LocalUser.SendMessage(replyTarget, ".add-library google \"username\" \"password\"");
        }
    }
}
