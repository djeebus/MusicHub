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

        static readonly string[] Formats = new string[] {
            "mp3",
            "m4a",
        };

		public IEnumerable<MusicHub.Song> GetSongs()
		{
            foreach (var format in Formats)
            {
                var files = System.IO.Directory.GetFiles(this._rootFolder, string.Format("*.{0}", format), System.IO.SearchOption.AllDirectories);

                foreach (var f in files)
                {
                    var s = this._metadataService.GetSongFromFilename(f);

                    s.ExternalId = f;

                    yield return s;
                }
            }
        }
    }
}