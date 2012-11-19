using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicHub
{
	public interface IMusicLibrary
	{
        event EventHandler<SongEventArgs> SongAdded;

		IEnumerable<Song> GetSongs();
		Song GetSong(string id);
		Song GetRandomSong();
	}
}
