using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MusicHub;
using System.Diagnostics;

namespace Website.Hubs
{
    public interface IClientProxy
    {
        void updateCurrentSong(MusicHub.Song song);
        void updateActiveUsers(IEnumerable<MusicHub.User> users);
        void updateStatus(MediaServerStatus status);
        void reportAddedSong(MusicHub.Song song);
        void log(string text);
    }

    public class ClientProxy : IClientProxy
    {
        dynamic _clients;
        IMediaServer _mediaServer;
        IMusicRepository _musicRepository;

        public ClientProxy(dynamic clients, IMediaServer mediaServer, IMusicRepository musicRepository)
        {
            this._clients = clients;
            this._mediaServer = mediaServer;
            this._musicRepository = musicRepository;

            this._mediaServer.SongStarted += _mediaServer_SongStarted;
            this._musicRepository.SongAdded += _musicRepository_SongAdded;
        }

        void _musicRepository_SongAdded(object sender, SongEventArgs e)
        {
            this.reportAddedSong(e.Song);
        }

        public void reportAddedSong(Song song)
        {
            this.log(string.Format("{0} by {1} has been added", song.Title, song.Artist));
        }

        protected void _mediaServer_SongStarted(object sender, SongEventArgs e)
        {
            this.updateCurrentSong(e.Song);
        }

        public void updateCurrentSong(MusicHub.Song song)
        {
            this._clients.updateCurrentSong(new
            {
                song = ConvertSong(song),
            });
        }

        public void updateActiveUsers(IEnumerable<MusicHub.User> users)
        {
            this._clients.updateActiveUsers(new
            {
                users = from u in users
                        select new
                        {
                            username = u.Username,
                            display = u.DisplayName,
                        }
            });
        }

        public void log(string text)
        {
            Trace.WriteLine(text, "LOG");

            this._clients.log(text);
        }

        public void updateStatus(MediaServerStatus status)
        {
            this._clients.updateStatus(new { status = status.ToString() });
        }

        private object ConvertSong(MusicHub.Song song)
        {
            if (song == null)
                return null;

            return new
            {
                title = song.Title,
                artist = song.Artist,
            };
        }
    }
}