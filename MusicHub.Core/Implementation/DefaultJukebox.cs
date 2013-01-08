using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MusicHub.Implementation
{
    public class DefaultJukebox : IJukebox
    {
        private readonly IMediaPlayer _mediaPlayer;
        private readonly ILibraryRepository _libraryRepository;
        private readonly IMusicLibraryFactory _musicLibraryFactory;
        private readonly ISongRepository _songRepository;
        private readonly IAffinityTracker _affinityTracker;
        private readonly SongSpider _spider;

        private List<string> _haters = new List<string>();

        public event EventHandler<SongEventArgs> SongStarted;

        public Song CurrentSong
        {
            get;
            private set;
        }

        public DefaultJukebox(
            IMediaPlayer mediaPlayer,
            IMusicLibraryFactory musicLibraryFactory,
            ILibraryRepository libraryRepository,
            ISongRepository songRepository,
            IAffinityTracker affinityTracker,
            SongSpider spider)
        {
            _mediaPlayer = mediaPlayer;
            _musicLibraryFactory = musicLibraryFactory;
            _libraryRepository = libraryRepository;
            _songRepository = songRepository;
            _spider = spider;
            _affinityTracker = affinityTracker;

            _mediaPlayer.SongFinished += _mediaPlayer_SongFinished;

            UpdateAllLibraries();
        }

        private void UpdateAllLibraries()
        {
            var libraries = _libraryRepository.GetLibraries();

            foreach (var libraryInfo in libraries)
            {
                this._spider.QueueLibrary(libraryInfo);
            }
        }

        private void OnSongStarted(Song song)
        {
            var handler = this.SongStarted;
            if (handler != null)
                handler(this, new SongEventArgs(song));

            this.CurrentSong = song;
            this._haters.Clear();
        }

        private void _mediaPlayer_SongFinished(object sender, EventArgs e)
        {
            var currentSong = this.CurrentSong;

            this._songRepository.MarkAsPlayed(currentSong.Id);
            this._libraryRepository.MarkAsPlayed(currentSong.LibraryId);

            this.SkipTrack();
        }

        public void Play()
        {
            if (CurrentSong != null)
                return; // already playing

            this.SkipTrack();
        }

        public void SkipTrack()
        {
            var currentSong = this.CurrentSong;

            var nextSong = _songRepository.GetRandomSong(currentSong);
            if (nextSong == null)
                return;

            this.PlaySong(nextSong);
        }

        public void PlaySong(Song song)
        {
            // need to stop first, since google music won't give you a 
            // new song's url while you're playing the old one
            this.Stop();

            var libraryInfo = _libraryRepository.GetLibrary(song.LibraryId);
            var library = _musicLibraryFactory.Create(libraryInfo);
            var songUrl = library.GetSongUrl(song.ExternalId);
            _mediaPlayer.PlaySong(song, songUrl);

            this.OnSongStarted(song);
        }

        public void Stop()
        {
            this.CurrentSong = null;

            _mediaPlayer.Stop();
        }

        protected int GetHatersNeededToSkip(int currentListeners)
        {
            return (int)Math.Ceiling(currentListeners * .5m);
        }

        public HateResult Hate(string userId, string songId, int currentListeners)
        {
            var hatersNeeded = GetHatersNeededToSkip(currentListeners);

            if (_haters.Contains(userId))
            {
                return new HateResult
                {
                    HatersNeeded = hatersNeeded - _haters.Count,
                };
            }

            _affinityTracker.Record(userId, songId, Affinity.Hate);

            _haters.Add(userId);
            var currentHaters = _haters.Count;

            var hatersLeft = hatersNeeded - currentHaters;

            if (hatersLeft > 0)
            {
                return new HateResult
                {
                    HatersNeeded = hatersLeft,
                };
            }

            this.SkipTrack();

            return new HateResult();
        }

        public void UpdateLibrary(string libraryId)
        {
            var libraryInfo = _libraryRepository.GetLibrary(libraryId);

            _spider.QueueLibrary(libraryInfo);
        }

        public void CreateLibrary(string userId, LibraryType libraryType, string path, string username, string password)
        {
            var info = _libraryRepository.Create(userId, libraryType, path, username, password);

            _spider.QueueLibrary(info);
        }

        public void DeleteLibrary(string libraryId)
        {
            _libraryRepository.Delete(libraryId);
        }

        public LibraryInfo[] GetLibrariesForUser(string userId)
        {
            return _libraryRepository.GetLibrariesForUser(userId);
        }

        public Song[] FindSongs(SearchType type, string term)
        {
            throw new NotImplementedException();
        }
    }
}
