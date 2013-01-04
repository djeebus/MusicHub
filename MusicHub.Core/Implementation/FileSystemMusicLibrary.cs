using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MusicHub.Implementation
{
	public class FileSystemMusicLibrary : MusicHub.IMusicLibrary
	{
		private readonly MusicHub.IMetadataService _metadataService;

        private readonly string _rootFolder;

		public FileSystemMusicLibrary(string rootFolder, MusicHub.IMetadataService metadataService)
		{
			this._metadataService = metadataService;
            this._rootFolder = rootFolder;
		}

        public string GetSongUrl(string externalId)
        {
            return externalId;
        }

		public IEnumerable<MusicHub.Song> GetSongs()
		{
            var files = System.IO.Directory.GetFiles(this._rootFolder, "*.mp3", System.IO.SearchOption.AllDirectories);

            foreach (var f in files)
            {
                var s = this._metadataService.GetSongFromFilename(f);

                s.ExternalId = f;

                yield return s;
            }
        }
    }
}