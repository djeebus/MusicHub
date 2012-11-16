using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicHub.Implementation
{
	public class DefaultSongSelector : ISongSelector
	{
		private readonly MusicHub.IMusicRepository _musicRepository;

		public DefaultSongSelector(MusicHub.IMusicRepository musicRepository)
		{
			this._musicRepository = musicRepository;
		}

		public Song GetRandomSong()
		{
			return this._musicRepository.GetRandomSong();
		}
	}
}
