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
                    this._libraryRepository.UpdateSyncResult(libraryInfo.Id, true, null);
                }
                catch (Exception ex)
                {
                    this._libraryRepository.UpdateSyncResult(libraryInfo.Id, false, ex.ToString());
                }
            }
        }

        private void Process(string libraryId, IMusicLibrary library)
        {
            this._libraryRepository.UpdateLastSyncDate(libraryId);

            Trace.WriteLine(string.Format("Getting songs for '{0}' ... ", libraryId), "SongSpider");
            foreach (var song in library.GetSongs())
            {
                this._songRespository.UpsertSong(libraryId, song.ExternalId, song.Artist, song.Title, song.Album, song.Track, song.Year);
            }

            Trace.WriteLine(string.Format("Pruning '{0}'", libraryId), "SongSpider");
            this._songRespository.PruneSongs(libraryId);
            Trace.WriteLine(string.Format("Finished pruning library '{0}', pruning", libraryId), "SongSpider");
        }
    }
}
