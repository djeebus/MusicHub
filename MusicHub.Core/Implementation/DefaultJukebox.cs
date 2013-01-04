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

        private int _haters = 0;

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
            this._haters = 0;
        }

        private void _mediaPlayer_SongFinished(object sender, EventArgs e)
        {
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
            var nextSong = _songRepository.GetRandomSong();
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

        public HateResult Hate(string userId, string songId, int currentListeners)
        {
            _affinityTracker.Record(userId, songId, Affinity.Hate);

            Interlocked.Increment(ref _haters);

            var percentageOfListenersHating = _haters / (decimal)currentListeners;

            if (percentageOfListenersHating < .5m)
            {
                return new HateResult
                {
                    HatersNeeded = (uint)Math.Ceiling(currentListeners * .5)
                };
            }

            this.SkipTrack();

            return new HateResult
            {
                HatersNeeded = 0,
            };
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
    }
}
