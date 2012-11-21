using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SignalR.Hubs;

namespace Website.Hubs
{
	public interface IMusicControl
	{
		void enqueueSong(string id);
		void requestStatus();
		void love();
		void hate();
        void stop();
        void createLibrary(string path);

		IClientProxy ClientProxy { get; }
	}

    [HubName("musicControl")]
	public class MusicControl : Hub, IDisconnect, IConnected, IMusicControl
	{
		private readonly MusicHub.IUserRepository _userRepository;
		private readonly MusicHub.IMediaPlayer _mediaServer;
		private readonly MusicHub.ISongSelector _songSelector;
        private readonly MusicHub.ILibraryRepository _libraryRepository;
        private readonly MusicHub.IConnectionRepository _connectionRepository;

		public MusicControl(
			MusicHub.IUserRepository userRepository,
			MusicHub.IMediaPlayer mediaServer,
			MusicHub.ISongSelector songSelector,
            MusicHub.ILibraryRepository libraryRepository,
            MusicHub.IConnectionRepository connectionRepository)
		{
			this._userRepository = userRepository;
			this._mediaServer = mediaServer;
			this._songSelector = songSelector;
            this._libraryRepository = libraryRepository;
            this._connectionRepository = connectionRepository;
		}

        public string UserId
        {
            get
            {
                var identity = Website.Models.MusicHubIdentity.CurrentIdentity;
                if (identity == null)
                    return null;

                return identity.User.Id;
            }
        }

        public System.Threading.Tasks.Task Disconnect()
        {
            this._connectionRepository.ClientDisconnected(this.Context.ConnectionId);

            return null;
        }

        public System.Threading.Tasks.Task Connect()
        {
            this._connectionRepository.ClientConnected(this.UserId, this.Context.ConnectionId);

            return null;
        }

        public System.Threading.Tasks.Task Reconnect(IEnumerable<string> groups)
        {
            this._connectionRepository.ClientConnected(this.UserId, this.Context.ConnectionId);

            return null;
        }

        private static object locker = new object();
        IClientProxy _clientProxy;
        public IClientProxy ClientProxy
        {
            get 
            {
                if (_clientProxy != null)
                    return _clientProxy;

                lock (locker)
                {
                    if (_clientProxy != null)
                        return _clientProxy;

                    this._clientProxy = new ClientProxy(this.Clients, this._mediaServer);
                }

                return this._clientProxy;
            }
        }

        public void requestStatus()
		{
			var users = this._userRepository.GetOnlineUsers();
			this.ClientProxy.updateActiveUsers(users);

            var libraries = this._libraryRepository.GetLibrariesForUser(this.UserId);
            this.ClientProxy.updateLibraries(libraries);

			var song = this._mediaServer.CurrentSong;
			this.ClientProxy.updateCurrentSong(song);

            var status = this._mediaServer.Status;
            this.ClientProxy.updateStatus(status);
		}

		public void hate()
		{
            var currentSong = this._mediaServer.CurrentSong;

			var nextSong = this._songSelector.GetRandomSong();

			this._mediaServer.PlaySong(nextSong);

			var user = this._userRepository.GetByName(this.Context.User.Identity.Name);

            this.ClientProxy.log(user.DisplayName + " hates " + currentSong.Title + " by " + currentSong.Artist);
		}

		public void enqueueSong(string id)
		{
            //this._mediaServer.PlaySong(song);

            //var user = _userRepository.GetByName(this.Context.User.Identity.Name);

            //this.ClientProxy.log(user.DisplayName + " has started playing " + song.Title + " by " + song.Artist);
		}

		public void love()
		{
		}

        public void stop()
        {
            this._mediaServer.Stop();
        }

        public void createLibrary(string path)
        {
            this._libraryRepository.Create(this.Context.User.Identity.Name, path);
        }
    }
}