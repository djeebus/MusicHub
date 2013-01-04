using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MusicHub
{
    public class SongSpider
    {
        private readonly Queue<LibraryInfo> _queue = new Queue<LibraryInfo>();
        
        private ILibraryRepository _libraryRepository;
        private ISongRepository _songRespository;
        private IMusicLibraryFactory _factory;
        
        private readonly Thread _thread;

        public string Status { get; private set; }

        public SongSpider(
            ILibraryRepository libraryRepository, 
            ISongRepository songRespository,
            IMusicLibraryFactory factory)
        {
            this._libraryRepository = libraryRepository;
            this._songRespository = songRespository;
            this._factory = factory;

            this._thread = new Thread(new ThreadStart(QueueWatcher));

            this._thread.Start();
        }

        public void QueueLibrary(LibraryInfo libraryInfo)
        {
            lock (_queue)
                _queue.Enqueue(libraryInfo);
        }

        private void QueueWatcher()
        {
            LibraryInfo libraryInfo;
            IMusicLibrary library;

            while (true)
            {
                Thread.Sleep(250); // don't spin!

                if (_queue.Count == 0)
                    continue;
                
                lock (_queue)
                    libraryInfo = _queue.Dequeue();

                library = _factory.Create(libraryInfo);

                try
                {
                    Process(libraryInfo.Id, library);
                }
                catch (Exception ex)
                {
                    this.Status = ex.ToString();
                }
            }
        }

        private void Process(string libraryId, IMusicLibrary library)
        {
            this._libraryRepository.UpdateLastSyncDate(libraryId);

            foreach (var song in library.GetSongs())
            {
                this._songRespository.UpsertSong(libraryId, song.ExternalId, song.Artist, song.Title, song.Album, song.Track, song.Year);
            }

            this._songRespository.PruneSongs(libraryId);
        }
    }
}
