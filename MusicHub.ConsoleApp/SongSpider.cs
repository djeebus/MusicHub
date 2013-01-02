using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MusicHub
{
    public class SongSpider
    {
        private readonly Queue<IMusicLibrary> _queue = new Queue<IMusicLibrary>();
        private readonly IKernel _kernel;
        
        private ILibraryRepository _libraryRepository;
        private ISongRepository _songRespository;
        
        private readonly Thread _thread;

        public string Status { get; private set; }

        public SongSpider(IKernel kernel)
        {
            this._kernel = kernel;

            this._thread = new Thread(new ThreadStart(QueueWatcher));

            this._thread.Start();
        }

        public void QueueLibrary(IMusicLibrary musicLibrary)
        {
            lock (_queue)
                _queue.Enqueue(musicLibrary);
        }

        private void QueueWatcher()
        {
            _songRespository = _kernel.Get<ISongRepository>();
            _libraryRepository = _kernel.Get<ILibraryRepository>();

            while (true)
            {
                Thread.Sleep(250); // don't spin!

                if (_queue.Count == 0)
                    continue;

                IMusicLibrary library;
                lock (_queue)
                    library = _queue.Dequeue();

                try
                {
                    Process(library);
                }
                catch (Exception ex)
                {
                    this.Status = ex.ToString();
                }
            }
        }

        private void Process(IMusicLibrary library)
        {
            this._libraryRepository.UpdateLastSyncDate(library.LibraryId);

            foreach (var song in library.GetSongs())
            {
                this._songRespository.UpsertSong(library.LibraryId, song.ExternalId, song.Artist, song.Title, song.Album, song.Track, song.Year);
            }

            this._songRespository.PruneSongs(library.LibraryId);
        }
    }
}
