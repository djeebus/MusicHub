using IrcDotNet.Samples.Common;
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
        private readonly IMediaPlayer _mediaPlayer;
        private readonly IJukebox _jukebox;

        public MusicHubBot(
            IJukebox jukebox,
            ILibraryRepository libraryRepository, 
            IUserRepository userRepository, 
            IMediaPlayer mediaPlayer,
            ISongRepository songRepository,
            IMetadataService metadataService)
        {
            _jukebox = jukebox;
            _userRepository = userRepository;
            _mediaPlayer = mediaPlayer;

            _jukebox.SongStarted += _jukebox_SongStarted;
        }

        void _jukebox_SongStarted(object sender, SongEventArgs e)
        {
            var msg = string.Format("Now playing: {0} by {1}", e.Song.Title, e.Song.Artist);

            SayInChannels(msg);
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

        protected override void InitializeChatCommandProcessors()
        {
            this.ChatCommandProcessors.Add("help", ProcessChatCommandHelp);
            this.ChatCommandProcessors.Add("add-library", ProcessChatCommandAddLibrary);
            this.ChatCommandProcessors.Add("list-libraries", ProcessChatCommandListLibraries);
            this.ChatCommandProcessors.Add("delete-library", ProcessChatCommandDeleteLibrary);
            this.ChatCommandProcessors.Add("sync-library", ProcessChatCommandSyncLibrary);

            //this.ChatCommandProcessors.Add("next", ProcessChatCommandNextSong);
            this.ChatCommandProcessors.Add("hate", ProcessChatCommandHateSong);
            this.ChatCommandProcessors.Add("stop", ProcessChatCommandStop);
            this.ChatCommandProcessors.Add("play", ProcessChatCommandPlay);
        }

        private void ProcessChatCommandPlay(IrcDotNet.IrcClient client, IrcDotNet.IIrcMessageSource source, IList<IrcDotNet.IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            var replyTarget = GetDefaultReplyTarget(client, source, targets);

            _jukebox.Play();
        }

        private void ProcessChatCommandStop(IrcDotNet.IrcClient client, IrcDotNet.IIrcMessageSource source, IList<IrcDotNet.IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            var replyTarget = GetDefaultReplyTarget(client, source, targets);

            //client.LocalUser.SendMessage(replyTarget, "Music has been stopped");
            SayInChannels(string.Format("{0} has stopped the music", source.Name));

            this._jukebox.Stop();
        }

        private void ProcessChatCommandNextSong(IrcDotNet.IrcClient client, IrcDotNet.IIrcMessageSource source, IList<IrcDotNet.IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            this._jukebox.SkipTrack();
        }

        private void ProcessChatCommandSyncLibrary(IrcDotNet.IrcClient client, IrcDotNet.IIrcMessageSource source, IList<IrcDotNet.IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            var user = _userRepository.EnsureUser(source.Name, source.Name);

            var replyTarget = GetDefaultReplyTarget(client, source, targets);

            parameters = NormalizeParameters(parameters);

            if (parameters.Count == 0)
            {
                InvalidSyncLibraryCommand(client, replyTarget, "Missing library id");
                return;
            }

            if (parameters.Count > 1)
            {
                InvalidSyncLibraryCommand(client, replyTarget, "Too many commands");
                return;
            }

            _jukebox.UpdateLibrary(parameters[0]);

            client.LocalUser.SendMessage(replyTarget, "Library sync has been queued");
        }

        private void InvalidSyncLibraryCommand(IrcDotNet.IrcClient client, IList<IrcDotNet.IIrcMessageTarget> replyTarget, string commandError)
        {
            client.LocalUser.SendMessage(replyTarget, string.Format("Invalid .sync-command: {0}", commandError));
            client.LocalUser.SendMessage(replyTarget, "Usage: .sync-library library-guid");
        }

        private void ProcessChatCommandHateSong(IrcDotNet.IrcClient client, IrcDotNet.IIrcMessageSource source, IList<IrcDotNet.IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            var replyTarget = GetDefaultReplyTarget(client, source, targets);

            var currentSong = _jukebox.CurrentSong;

            if (currentSong == null)
            {
                client.LocalUser.SendMessage(replyTarget, "There is no song playing; please do not hate indiscriminately");
                return;
            }

            SayInChannels(string.Format("{0} hates the current song", source.Name));

            var user = this._userRepository.GetByName(source.Name);
            var song = this._jukebox.CurrentSong;

            var result = _jukebox.Hate(user.Id, song.Id, this.Clients.Sum(c => c.Channels.Sum(ch => ch.Users.Count)));

            if (result.HatersNeeded != 0)
                SayInChannels(string.Format("{0} more haters needed to skip the track!", result.HatersNeeded));
        }

        List<string> _haters = new List<string>();

        private void ProcessChatCommandDeleteLibrary(IrcDotNet.IrcClient client, IrcDotNet.IIrcMessageSource source, IList<IrcDotNet.IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            var user = _userRepository.EnsureUser(source.Name, source.Name);

            var replyTarget = GetDefaultReplyTarget(client, source, targets);

            parameters = NormalizeParameters(parameters);

            if (parameters.Count == 0)
            {
                InvalidDeleteLibraryCommand(client, replyTarget, "Missing library id");
                return;
            }

            if (parameters.Count > 1)
            {
                InvalidDeleteLibraryCommand(client, replyTarget, "Too many commands");
                return;
            }

            var libraryId = parameters[0];
            this._jukebox.DeleteLibrary(libraryId);

            client.LocalUser.SendMessage(replyTarget, "Library has been deleted");
        }

        private void InvalidDeleteLibraryCommand(IrcDotNet.IrcClient client, IList<IrcDotNet.IIrcMessageTarget> replyTarget, string commandError)
        {
            client.LocalUser.SendMessage(replyTarget, string.Format("Invalid .delete-command: {0}", commandError));
            client.LocalUser.SendMessage(replyTarget, "Usage: .delete-library library-guid");
        }

        private void ProcessChatCommandListLibraries(IrcDotNet.IrcClient client, IrcDotNet.IIrcMessageSource source, IList<IrcDotNet.IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            var user = _userRepository.EnsureUser(source.Name, source.Name);

            var replyTarget = GetDefaultReplyTarget(client, source, targets);

            var libraries = this._jukebox.GetLibrariesForUser(user.Id);

            client.LocalUser.SendMessage(replyTarget, "Your libraries: ");
            foreach (var library in libraries)
            {
                client.LocalUser.SendMessage(replyTarget, string.Format("  {0} (songs: {1}) (id: {2})", library.Name, library.TotalSongs, library.Id));
            }
        }

        private void ProcessChatCommandAddLibrary(IrcDotNet.IrcClient client, IrcDotNet.IIrcMessageSource source, IList<IrcDotNet.IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            var user = _userRepository.EnsureUser(source.Name, source.Name);

            var replyTarget = GetDefaultReplyTarget(client, source, targets);

            parameters = NormalizeParameters(parameters);

            if (parameters.Count < 2)
            {
                InvalidAddLibraryCommand(client, replyTarget);
                return;
            }

            var type = parameters[0];
            switch ((type ?? string.Empty).ToLower())
            {
                case "folder":
                    if (parameters.Count != 2)
                    {
                        InvalidAddLibraryCommand(client, replyTarget);
                        return;
                    }

                    var path = parameters[1];
                    _jukebox.CreateLibrary(user.Id, LibraryType.SharedFolder, path, null, null);

                    client.LocalUser.SendMessage(replyTarget, string.Format("'{0}' has been added as a library, now syncing ...", path));
                    break;

                case "google":
                    if (parameters.Count != 3)
                    {
                        InvalidAddLibraryCommand(client, replyTarget);
                        return;
                    }

                    var username = parameters[1];
                    var password = parameters[2];


                    _jukebox.CreateLibrary(user.Id, LibraryType.GoogleMusic, null, username, password);
                    client.LocalUser.SendMessage(replyTarget, string.Format("{0}'s google music library has been added as a library, now syncing ...", username));
                    break;

                default:
                    InvalidAddLibraryCommand(client, replyTarget);
                    return;
            }
        }

        const char Delimiter = '"';
        private IList<string> NormalizeParameters(IList<string> parameters)
        {
            var list = new List<string>();
            string partial = null;
            foreach (var p in parameters)
            {
                if (string.IsNullOrWhiteSpace(p))
                    continue;


                if (partial != null)
                {
                    partial += " " + p;

                    if (partial.Last() == Delimiter)
                    {
                        list.Add(partial.Substring(0, partial.Length - 1));
                        partial = null;
                    }
                    continue;
                }

                if (p[0] == Delimiter)
                {
                    if (p.Last() != Delimiter)
                    {
                        partial = p.Substring(1);
                        continue;
                    }

                    list.Add(p.Substring(1, p.Length - 2));
                    continue;
                }

                list.Add(p);
            }

            if (partial != null)
                throw new ArgumentOutOfRangeException("parameters", partial, "Missing final delimiter for argument");

            return list;
        }

        private static void InvalidAddLibraryCommand(IrcDotNet.IrcClient client, IList<IrcDotNet.IIrcMessageTarget> replyTarget)
        {
            client.LocalUser.SendMessage(replyTarget, "Invalid command");
            client.LocalUser.SendMessage(replyTarget, ".add-library folder \"path\"");
            client.LocalUser.SendMessage(replyTarget, ".add-library google \"username\" \"password\"");
        }

        private void ProcessChatCommandHelp(IrcDotNet.IrcClient client, IrcDotNet.IIrcMessageSource source, IList<IrcDotNet.IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            if (parameters.Count != 0)
                throw new InvalidCommandParametersException(1);

            // List all commands recognized by this bot.
            var replyTarget = GetDefaultReplyTarget(client, source, targets);
            client.LocalUser.SendMessage(replyTarget, "Commands recognized by bot:");
            client.LocalUser.SendMessage(replyTarget, string.Join(", ",
                this.ChatCommandProcessors.Select(kvPair => kvPair.Key)));
        }

        protected override void OnClientConnect(IrcDotNet.IrcClient client)
        {
        }

        protected override void OnClientDisconnect(IrcDotNet.IrcClient client)
        {
        }

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

        protected override void OnChannelUserJoined(IrcDotNet.IrcChannel channel, IrcDotNet.IrcChannelUserEventArgs e)
        {
        }

        protected override void OnChannelUserLeft(IrcDotNet.IrcChannel channel, IrcDotNet.IrcChannelUserEventArgs e)
        {
        }

        protected override void OnChannelNoticeReceived(IrcDotNet.IrcChannel channel, IrcDotNet.IrcMessageEventArgs e)
        {
        }

        protected override void OnChannelMessageReceived(IrcDotNet.IrcChannel channel, IrcDotNet.IrcMessageEventArgs e)
        {
        }
    }
}
