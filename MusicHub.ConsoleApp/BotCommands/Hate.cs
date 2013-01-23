using MusicHub.ConsoleApp.Irc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicHub.ConsoleApp.BotCommands
{
    class Hate : BaseCommand
    {
        public override string Command
        {
            get { return "hate"; }
        }

        private readonly List<string> _haters = new List<string>();
        private readonly IUserRepository _userRepository;
        private readonly IJukebox _jukebox;

        public Hate(IUserRepository userRepository, IJukebox jukebox)
        {
            _userRepository = userRepository;
            _jukebox = jukebox;
        }

        public override void ExecuteCommand(IrcDotNet.IrcClient client, IrcDotNet.IIrcMessageSource source, IList<IrcDotNet.IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            IrcHelper.SayInChannels(client, string.Format("{0} hates this song", source.Name));

            var user = this._userRepository.EnsureUser(source.Name, source.Name);

            HateResult result;
            try
            {
                result = _jukebox.Hate(user.Id);
            }
            catch
            {
                client.LocalUser.SendMessage(targets, "There is no song playing; please do not hate indiscriminately");
                return;
            }

            if (result.HatersNeeded != 0)
                IrcHelper.SayInChannels(client, string.Format("{0} more haters needed to skip the track!", result.HatersNeeded));
        }
    }
}
