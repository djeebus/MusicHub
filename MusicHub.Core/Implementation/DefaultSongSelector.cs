using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicHub.Implementation
{
	public class DefaultSongSelector : ISongSelector
	{
		private readonly MusicHub.ISongRepository _songRepository;

        public DefaultSongSelector(MusicHub.ISongRepository songRepository)
		{
			this._songRepository = songRepository;
		}

		public Song GetRandomSong()
		{
			return this._songRepository.GetRandomSong();
		}
	}
}
