using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly IUserRepository _userRepository;
        private readonly SongSpider _spider;

        private List<string> _haters = new List<string>();

        public event EventHandler<SongEventArgs> SongStarting;
        public event EventHandler<SongEventArgs> SongStarted;
        public event EventHandler<SongEventArgs> SongFinished;

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
            IUserRepository userRepository,
            SongSpider spider)
        {
            _mediaPlayer = mediaPlayer;
            _musicLibraryFactory = musicLibraryFactory;
            _libraryRepository = libraryRepository;
            _songRepository = songRepository;
            _spider = spider;
            _affinityTracker = affinityTracker;
            _userRepository = userRepository;

            _mediaPlayer.SongFinished += _mediaPlayer_SongFinished;

            UpdateAllLibraries();
            MarkAllUsersAsAway();
        }

        private void MarkAllUsersAsAway()
        {
            this._songRepository.MarkUsersAsAway();
        }

        private void UpdateAllLibraries()
        {
            var libraries = _libraryRepository.GetLibraries();

            foreach (var libraryInfo in libraries)
            {
                this._spider.QueueLibrary(libraryInfo);
            }
        }

        private void OnSongStarting(Song song)
        {
            var handler = this.SongStarting;
            if (handler != null)
                handler(this, new SongEventArgs(song));
        }


        private void OnSongStarted(Song song)
        {
            var handler = this.SongStarted;
            if (handler != null)
                handler(this, new SongEventArgs(song));

            this.CurrentSong = song;
            this._haters.Clear();
        }

        private void OnSongFinished(Song song)
        {
            var handler = this.SongFinished;
            if (handler != null)
                handler(this, new SongEventArgs(song));
        }

        private void _mediaPlayer_SongFinished(object sender, EventArgs e)
        {
            var currentSong = this.CurrentSong;
            if (currentSong == null)
                return;

            this._songRepository.MarkAsPlayed(currentSong.Id);
            this._libraryRepository.MarkAsPlayed(currentSong.LibraryId, true);

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

            while (true)
            {
                var nextSong = _songRepository.GetRandomSong(currentSong);
                if (nextSong == null)
                    return;

                try
                {
                    this.PlaySong(nextSong);
                    break;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(string.Format("Error playing '{0}': {1}", nextSong.Id, ex), "DefaultJukebox");
                    _libraryRepository.MarkAsPlayed(nextSong.LibraryId, false);
                }
            }
        }

        public void PlaySong(Song song)
        {
            // need to stop first, since google music won't give you a 
            // new song's url while you're playing the old one
            this.Stop();

            var libraryInfo = _libraryRepository.GetLibrary(song.LibraryId);
            var library = _musicLibraryFactory.Create(libraryInfo);
            var songUrl = library.GetSongUrl(song.ExternalId);

            this.OnSongStarting(song);

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

        public HateResult Hate(string userId)
        {
            var currentListeners = _userRepository.GetOnlineUsers();

            var hatersNeeded = GetHatersNeededToSkip(currentListeners.Length);

            // bail if user has already hated the song
            if (_haters.Contains(userId))
            {
                return new HateResult
                {
                    HatersNeeded = hatersNeeded - _haters.Count,
                };
            }

            var currentSong = this.CurrentSong;
            if (currentSong == null)
                throw new Exception("Cannot hate when there is no song playing");

            _affinityTracker.Record(userId, currentSong.Id, Affinity.Hate);

            if (currentSong.UserId == userId)
            {
                this.SkipTrack();
                return new HateResult();
            }

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

        public void Love(string userId)
        {
            var currentSong = this.CurrentSong;
            if (currentSong == null)
                throw new Exception("Cannot hate when there is no song playing");

            _affinityTracker.Record(userId, currentSong.Id, Affinity.Hate);
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
