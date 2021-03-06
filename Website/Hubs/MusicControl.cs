﻿using System;
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
        void createSharedFolderLibrary(string path);
        void createGoogleMusicLibrary(string username, string password);

		IClientProxy ClientProxy { get; }
	}

    [HubName("musicControl")]
	public class MusicControl : Hub, IDisconnect, IConnected, IMusicControl
	{
		private readonly MusicHub.IUserRepository _userRepository;
		private readonly MusicHub.IMediaPlayer _mediaServer;
        private readonly MusicHub.ILibraryRepository _libraryRepository;
        private readonly MusicHub.IConnectionRepository _connectionRepository;
        private readonly MusicHub.ISongRepository _songRepository;
        private readonly Website.Models.MediaLibraryFactory _mediaLibraryFactory;
        private readonly MusicHub.IJukebox _jukebox;

		public MusicControl(
			MusicHub.IUserRepository userRepository,
			MusicHub.IMediaPlayer mediaServer,
            MusicHub.ILibraryRepository libraryRepository,
            MusicHub.IConnectionRepository connectionRepository,
            MusicHub.ISongRepository songRepository,
            MusicHub.IJukebox jukebox,
            Website.Models.MediaLibraryFactory mediaLibraryFactory)
		{
			this._userRepository = userRepository;
			this._mediaServer = mediaServer;
            this._libraryRepository = libraryRepository;
            this._connectionRepository = connectionRepository;
            this._songRepository = songRepository;
            this._mediaLibraryFactory = mediaLibraryFactory;
            this._jukebox = jukebox;
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

                    this._clientProxy = new ClientProxy(this.Clients, this._jukebox);
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

			var song = this._jukebox.CurrentSong;
			this.ClientProxy.updateCurrentSong(song);
		}

		public void hate()
		{
			var user = this._userRepository.EnsureUser(this.Context.User.Identity.Name, this.Context.User.Identity.Name);

            var currentSong = this._jukebox.CurrentSong;

            this._jukebox.Hate(user.Id);

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
            this._jukebox.Love(this.UserId);
		}

        public void play()
        {
            this._jukebox.Play();
        }

        public void stop()
        {
            this._mediaServer.Stop();
        }

        public void createSharedFolderLibrary(string path)
        {
            this._libraryRepository.Create(this.Context.User.Identity.Name, MusicHub.LibraryType.SharedFolder, path, null, null);
        }

        public void createGoogleMusicLibrary(string username, string password)
        {
            this._libraryRepository.Create(this.Context.User.Identity.Name, MusicHub.LibraryType.GoogleMusic, null, username, password);
        }
    }
}