using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MusicHub.Implementation
{
	public class FileSystemMusicLibrary : MusicHub.IMusicLibrary
	{
		private static readonly object locker = new object();
		private static readonly Random random = new Random();

		private readonly MusicHub.IMetadataService _metadataService;

		private static List<MusicHub.Song> songs = null;

		public FileSystemMusicLibrary(string rootFolder, MusicHub.IMetadataService metadataService)
		{
			this._metadataService = metadataService;

			var files = System.IO.Directory.GetFiles(rootFolder, "*.mp3", System.IO.SearchOption.AllDirectories);

			if (songs == null)
			{
				lock (locker)
				{
					if (songs == null)
					{
						var temp = new List<MusicHub.Song>();
						int index = 0;
						foreach (var f in files)
						{
							var s = this._metadataService.GetSongFromFilename(f);

							s.Id = (index++).ToString();
							s.Filename = f;

							temp.Add(s);

                            this.OnSongAdded(s);
						}

						songs = temp;
					}
				}
			}
		}

        private void OnSongAdded(MusicHub.Song s)
        {
            var handler = this.SongAdded;
            if (handler != null)
                handler(this, new MusicHub.SongEventArgs(s));
        }

		public MusicHub.Song GetSong(string id)
		{
			var index = int.Parse(id);

			return songs[index];
		}

		public IEnumerable<MusicHub.Song> GetSongs()
		{
			return songs;
		}

		public MusicHub.Song GetRandomSong()
		{
			var totalSongs = songs.Count;

			var index = random.Next(totalSongs);

			return songs[index];
		}

        public event EventHandler<MusicHub.SongEventArgs> SongAdded;
    }
}