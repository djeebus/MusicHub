using System;
using System.Collections.Generic;
using System.Diagnostics;
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

                try
                {
                    library = _factory.Create(libraryInfo);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(string.Format("Error creating library {0}: {1}", libraryInfo.Id, ex), "SongSpider");
                    continue;
                }

                try
                {
                    Process(libraryInfo.Id, library);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(string.Format("Error processing library {0}: {1}", libraryInfo.Id, ex), "SongSpider");
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
