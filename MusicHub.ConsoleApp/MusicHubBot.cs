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
        private readonly ILibraryRepository _libraryRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMediaPlayer _mediaPlayer;
        private readonly ISongRepository _songRepository;
        private readonly IMetadataService _metadataService;
        private readonly SongSpider _spider;

        public MusicHubBot(
            ILibraryRepository libraryRepository, 
            IUserRepository userRepository, 
            IMediaPlayer mediaPlayer,
            ISongRepository songRepository,
            IMetadataService metadataService,
            SongSpider spider)
        {
            _libraryRepository = libraryRepository;
            _userRepository = userRepository;
            _mediaPlayer = mediaPlayer;
            _songRepository = songRepository;
            _metadataService = metadataService;
            _spider = spider;

            _mediaPlayer.SongStarted += _mediaPlayer_SongStarted;
            _mediaPlayer.StatusChanged += _mediaPlayer_StatusChanged;

            this.SpiderAllLibraries();
        }

        void _mediaPlayer_StatusChanged(object sender, StatusEventArgs e)
        {
            switch (e.Status)
            {
                case MediaPlayerStatus.SongFinished:
                    this.PlayNextSong(); // triggers even if a song was intentionally skipped
                    break;
            }
        }

        private void SpiderAllLibraries()
        {
            var libraries = _libraryRepository.GetLibraries();

            foreach (var libraryInfo in libraries)
            {
                var library = CreateLibrary(libraryInfo);

                this._spider.QueueLibrary(library);
            }
        }

        void _mediaPlayer_SongStarted(object sender, SongEventArgs e)
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

            var status = this._mediaPlayer.Status;
            if (status == MediaPlayerStatus.Playing)
            {
                client.LocalUser.SendMessage(replyTarget, "Music is already playing - stop being redundant!");
                return;
            }

            //client.LocalUser.SendMessage(replyTarget, "Music has started playing");
            SayInChannels(string.Format("{0} has started playing music", source.Name));

            this.PlayNextSong();
        }

        private void ProcessChatCommandStop(IrcDotNet.IrcClient client, IrcDotNet.IIrcMessageSource source, IList<IrcDotNet.IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            var replyTarget = GetDefaultReplyTarget(client, source, targets);

            //client.LocalUser.SendMessage(replyTarget, "Music has been stopped");
            SayInChannels(string.Format("{0} has stopped the music", source.Name));

            this._mediaPlayer.Stop();
        }

        private void ProcessChatCommandNextSong(IrcDotNet.IrcClient client, IrcDotNet.IIrcMessageSource source, IList<IrcDotNet.IIrcMessageTarget> targets, string command, IList<string> parameters)
        {
            this.PlayNextSong();
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

            var libraryInfo = _libraryRepository.GetLibrary(parameters[0]);
            var library = CreateLibrary(libraryInfo);

            _spider.QueueLibrary(library);

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

            if (_mediaPlayer.Status != MediaPlayerStatus.Playing)
            {
                client.LocalUser.SendMessage(replyTarget, "There is no song playing; please do not hate indiscriminately");
                return;
            }

            var currentSong = _mediaPlayer.CurrentSong;
            if (currentSong == null)
                throw new ArgumentNullException("currentSong");

            SayInChannels(string.Format("{0} hates the current song", source.Name));

            PlayNextSong();
        }

        List<string> _haters = new List<string>();
        private void PlayNextSong()
        {
            // need to stop first, since google music won't give you a 
            // new song's url while you're playing the old one
            this._mediaPlayer.Stop();

            var nextSong = _songRepository.GetRandomSong();
            if (nextSong == null)
                return;

            var libraryInfo = _libraryRepository.GetLibrary(nextSong.LibraryId);
            var library = CreateLibrary(libraryInfo);
            var songUrl = library.GetSongUrl(nextSong.ExternalId);
            _mediaPlayer.PlaySong(nextSong, songUrl);
        }

        private IMusicLibrary CreateLibrary(LibraryInfo libraryInfo)
        {
            switch (libraryInfo.Type)
            {
                case LibraryType.GoogleMusic:
                    return new GooglePlay.GoogleMusicMusicLibrary(libraryInfo.Id, libraryInfo.Username, libraryInfo.Password);

                case LibraryType.SharedFolder:
                    return new Implementation.FileSystemMusicLibrary(libraryInfo.Id, libraryInfo.Location, _metadataService);

                default:
                    throw new ArgumentOutOfRangeException("libraryInfo.Type", libraryInfo.Type, "Unknown type");
            }
        }

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
            this._libraryRepository.Delete(libraryId);

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

            var libraries = this._libraryRepository.GetLibrariesForUser(user.Id);

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
            LibraryInfo libraryInfo;
            switch ((type ?? string.Empty).ToLower())
            {
                case "folder":
                    if (parameters.Count != 2)
                    {
                        InvalidAddLibraryCommand(client, replyTarget);
                        return;
                    }

                    var path = parameters[1];
                    libraryInfo = _libraryRepository.Create(user.Id, LibraryType.SharedFolder, path, null, null);

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

                    libraryInfo = _libraryRepository.Create(user.Id, LibraryType.GoogleMusic, null, username, password);
                    client.LocalUser.SendMessage(replyTarget, string.Format("{0}'s google music library has been added as a library, now syncing ...", username));
                    break;

                default:
                    InvalidAddLibraryCommand(client, replyTarget);
                    return;
            }

            var library = CreateLibrary(libraryInfo);

            _spider.QueueLibrary(library);

            SayInChannels(string.Format("{0} has added a {1} library", source.Name, libraryInfo.Type));
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
            this.PlayNextSong();
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
