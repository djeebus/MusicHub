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

		IClientProxy ClientProxy { get; }
	}

    [HubName("musicControl")]
	public class MusicControl : Hub, IDisconnect, IConnected, IMusicControl
	{
		private readonly MusicHub.IMusicRepository _musicRepository;
		private readonly MusicHub.IUserRepository _userRepository;
		private readonly MusicHub.IMediaServer _mediaServer;
		private readonly MusicHub.ISongSelector _songSelector;

		public MusicControl(
			MusicHub.IMusicRepository musicRepository,
			MusicHub.IUserRepository userRepository,
			MusicHub.IMediaServer mediaServer,
			MusicHub.ISongSelector songSelector)
		{
			this._musicRepository = musicRepository;
			this._userRepository = userRepository;
			this._mediaServer = mediaServer;
			this._songSelector = songSelector;
		}

		public void requestStatus()
		{
			var users = this._userRepository.GetOnlineUsers();
			this.ClientProxy.updateActiveUsers(users);

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

			var user = this._userRepository.Get(this.Context.User.Identity.Name);

            this.ClientProxy.log(user.DisplayName + " hates " + currentSong.Title + " by " + currentSong.Artist);
		}

		public void enqueueSong(string id)
		{
			var song = _musicRepository.GetSong(id);

			this._mediaServer.PlaySong(song);

			var user = _userRepository.Get(this.Context.User.Identity.Name);

			this.ClientProxy.log(user.DisplayName + " has started playing " + song.Title + " by " + song.Artist);
		}

		public System.Threading.Tasks.Task Disconnect()
		{
			var user = this._userRepository.ClientDisconnected(this.Context.ConnectionId);

			this.ClientProxy.log(user.DisplayName + " has disconnected");

			return null;
		}

		public System.Threading.Tasks.Task Connect()
		{
			var user = this._userRepository.ClientConnected(this.Context.User.Identity.Name, this.Context.ConnectionId);

			this.ClientProxy.log(user.DisplayName + " has connected");

			return null;
		}

		public System.Threading.Tasks.Task Reconnect(IEnumerable<string> groups)
		{
			this._userRepository.ClientConnected(this.Context.User.Identity.Name, this.Context.ConnectionId);

			return null;
			// not sure what to do here, seems to be really chatty though
			//var user = this._userRepository.Get(this.Context.User.Identity.Name);
			//return this.Clients.log(user.DisplayName + " has reconnected");
		}

		public void love()
		{
		}

        public void stop()
        {
            this._mediaServer.Stop();
        }

		public IClientProxy ClientProxy
		{
			get { return new ClientProxy(this.Clients, this._mediaServer, this._musicRepository); }
		}
    }
}
