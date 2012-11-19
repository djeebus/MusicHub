using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicHub
{
	public interface IMediaPlayer
	{
		Song CurrentSong { get; }
        MediaPlayerStatus Status { get; }

		void PlaySong(Song song);
		void Stop();

		event EventHandler<SongEventArgs> SongStarted;
	}

	public class SongEventArgs : EventArgs
	{
		public Song Song { get; private set; }

		public SongEventArgs(Song song)
		{
			this.Song = song;
		}
	}

    public enum MediaPlayerStatus
    {
        Playing,
        Stopped,
    }
}
