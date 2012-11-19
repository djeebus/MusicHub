using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicHub.Implementation
{
	public class DefaultSongSelector : ISongSelector
	{
		private readonly MusicHub.IMusicLibrary _musicRepository;

		public DefaultSongSelector(MusicHub.IMusicLibrary musicRepository)
		{
			this._musicRepository = musicRepository;
		}

		public Song GetRandomSong()
		{
			return this._musicRepository.GetRandomSong();
		}
	}
}
