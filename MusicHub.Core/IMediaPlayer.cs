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

	public class SongEventArgs : EventArgs
	{
		public Song Song { get; private set; }

		public SongEventArgs(Song song)
		{
			this.Song = song;
		}
	}

    public class StatusEventArgs : EventArgs
    {
        public MediaPlayerStatus Status { get; private set; }

        public StatusEventArgs(MediaPlayerStatus status)
        {
            this.Status = status;
        }
    }

    public enum MediaPlayerStatus
    {
        Playing,
        SongFinished,
        Stopped,
    }
}
