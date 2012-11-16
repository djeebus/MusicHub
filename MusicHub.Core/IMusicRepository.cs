using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicHub
{
	public interface IMusicRepository
	{
        event EventHandler<SongEventArgs> SongAdded;

		IEnumerable<Song> GetSongs();
		Song GetSong(string id);
		Song GetRandomSong();
	}
}
