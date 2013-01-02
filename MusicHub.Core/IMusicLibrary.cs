using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicHub
{
	public interface IMusicLibrary
	{
        string LibraryId { get; }
		IEnumerable<Song> GetSongs();
        string GetSongUrl(string externalId);
	}
}
