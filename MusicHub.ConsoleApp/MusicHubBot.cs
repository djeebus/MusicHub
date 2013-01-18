using IrcDotNet.Samples.Common;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MusicHub.ConsoleApp
{
    public class MusicHubBot : IrcBot
    {
        private readonly IUserRepository _userRepository;
        private readonly ILibraryRepository _libraryRepository;
        private readonly IMediaPlayer _mediaPlayer;
        private readonly IJukebox _jukebox;
        private readonly IKernel _kernel;

        public MusicHubBot(
            IJukebox jukebox,
            ILibraryRepository libraryRepository, 
            IUserRepository userRepository, 
            IMediaPlayer mediaPlayer,
            ISongRepository songRepository,
            IMetadataService metadataService, 
            IKernel kernel)
        {
            _jukebox = jukebox;
            _userRepository = userRepository;
            _libraryRepository = libraryRepository;
            _mediaPlayer = mediaPlayer;
            _kernel = kernel;

            _jukebox.SongStarted += _jukebox_SongStarted;
        }

        void _jukebox_SongStarted(object sender, SongEventArgs e)
        {
            SayInChannels(e.Song.ToString());
        }

        private void SayInChannels(string msg)
        {
            foreach (var client in this.Clients)
            {
                foreach (var channel in client.Channels)
                {
                    client.LocalUser.SendMessage(channel, msg);
                }
            }
        }

        protected override void InitializeCommandProcessors()
        {
        }

        private readonly List<BotCommands.ICommand> _commands = new List<BotCommands.ICommand>();
        protected override void InitializeChatCommandProcessors()
        {
            _commands.AddRange(new BotCommands.ICommand[] {
                _kernel.Get<BotCommands.AddLibrary>(),
                _kernel.Get<BotCommands.DeleteLibrary>(),
                _kernel.Get<BotCommands.Hate>(),
                _kernel.Get<BotCommands.ListLibraries>(),
                _kernel.Get<BotCommands.Love>(),
                _kernel.Get<BotCommands.Skip>(),
                _kernel.Get<BotCommands.Play>(),
                _kernel.Get<BotCommands.Stop>(),
                _kernel.Get<BotCommands.SyncLibrary>(),
            });

            foreach (var command in _commands)
                this.ChatCommandProcessors.Add(command.Command, command.ExecuteCommand);

            this.ChatCommandProcessors.Add("help", ProcessChatCommandHelp);
        }

        private void ProcessChatCommandHelp(IrcDotNet.IrcClient client, IrcDotNet.IIrcMessageSource source, IList<IrcDotNet.IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            if (parameters.Count != 0)
                throw new InvalidCommandParametersException(1);

            // List all commands recognized by this bot.
            client.LocalUser.SendMessage(targets, "Commands recognized by bot:");
            client.LocalUser.SendMessage(targets, string.Join(", ",
                this.ChatCommandProcessors.Select(kvPair => kvPair.Key)));
        }

        protected override void OnClientConnect(IrcDotNet.IrcClient client)
        {
        }

        protected override void OnClientDisconnect(IrcDotNet.IrcClient client)
        {
            var handler = this.ClientDisconnect;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public event EventHandler ClientDisconnect;

        protected override void OnClientRegistered(IrcDotNet.IrcClient client)
        {
            client.Channels.Join("#music");
        }

        protected override void OnLocalUserJoinedChannel(IrcDotNet.IrcLocalUser localUser, IrcDotNet.IrcChannelEventArgs e)
        {
            this._jukebox.Play();
        }

        protected override void OnLocalUserLeftChannel(IrcDotNet.IrcLocalUser localUser, IrcDotNet.IrcChannelEventArgs e)
        {
        }

        protected override void OnLocalUserNoticeReceived(IrcDotNet.IrcLocalUser localUser, IrcDotNet.IrcMessageEventArgs e)
        {
        }

        protected override void OnLocalUserMessageReceived(IrcDotNet.IrcLocalUser localUser, IrcDotNet.IrcMessageEventArgs e)
        {
        }

        // todo: fix bug that flips these messages
        protected override void OnChannelUserJoined(IrcDotNet.IrcChannel channel, IrcDotNet.IrcChannelUserEventArgs e)
        {
            var user = this._userRepository.EnsureUser(e.ChannelUser.User.UserName, null);

            this._userRepository.MarkAsOnline(user.Id, false);
        }

        // todo: fix bug that flips these messages
        protected override void OnChannelUserLeft(IrcDotNet.IrcChannel channel, IrcDotNet.IrcChannelUserEventArgs e)
        {
            var user = this._userRepository.EnsureUser(e.ChannelUser.User.UserName, null);

            this._userRepository.MarkAsOnline(user.Id, true);

            Action<string> say = msg => channel.Client.LocalUser.SendMessage(e.ChannelUser.User, msg);

            say("The dev team 3 music bot welcomes you!");
            say(string.Format("To listen to the stream, point your media player at 'http://{0}:{1}/'", Environment.MachineName, MusicHub.BassNet.BassNetMediaPlayer.PortNumber));
            say("For more instructions, type '.help'. Remember to message me directly and not in the chat channel!");

            var libraries = this._libraryRepository.GetLibrariesForUser(user.Id);
            var brokenLibraries = libraries.Where(l => !string.IsNullOrWhiteSpace(l.ErrorMessage));
            foreach (var library in brokenLibraries)
            {
                say(string.Format("The connection to '{0}' failed!", library.Name));
                say(string.Format("The error message was: {0}", library.ErrorMessage));
                say(string.Format("After you fix the problem, say '.sync-library {0}' to get the music going again", library.Id));
            }
        }

        protected override void OnChannelNoticeReceived(IrcDotNet.IrcChannel channel, IrcDotNet.IrcMessageEventArgs e)
        {
        }

        protected override void OnChannelMessageReceived(IrcDotNet.IrcChannel channel, IrcDotNet.IrcMessageEventArgs e)
        {
        }
    }
}
