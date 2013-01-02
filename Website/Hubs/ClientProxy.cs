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
        void reportAddedSong(MusicHub.Song song);
        void log(string text);

        void updateCurrentSong(MusicHub.Song song);
        void updateActiveUsers(IEnumerable<MusicHub.User> users);
        void updateStatus(MediaPlayerStatus status);
        void updateLibraries(LibraryInfo[] libraries);
    }

    public class ClientProxy : IClientProxy
    {
        dynamic _clients;
        IMediaPlayer _mediaServer;

        public ClientProxy(dynamic clients, IMediaPlayer mediaServer)
        {
            this._clients = clients;
            this._mediaServer = mediaServer;

            this._mediaServer.SongStarted += _mediaServer_SongStarted;
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

        public void updateStatus(MediaPlayerStatus status)
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

        public void updateLibraries(LibraryInfo[] libraries)
        {
            this._clients.updateLibraries(libraries.Select(ConvertLibrary));
        }

        private object ConvertLibrary(LibraryInfo library)
        {
            return new
            {
                id = library.Id,
                name = library.Name,
                songCount = library.TotalSongs,
            };
        }
    }
}