using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicHub
{
	public interface IMediaPlayer
	{
		void PlaySong(Song song, string mediaUrl);
		void Stop();

        event EventHandler SongFinished;
	}
}
