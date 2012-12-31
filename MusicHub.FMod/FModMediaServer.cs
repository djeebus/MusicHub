using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicHub.FMod
{
	public class FModMediaServer : IMediaPlayer
	{
		const int MAX_CHANNELS = 32;

		private readonly FMOD.System system;

		private FMOD.Channel channel;
		private Song currentSong;

		public FModMediaServer()
		{
			FMOD.RESULT result;

			uint version = 0;

			result = FMOD.Factory.System_Create(ref system);
			ErrorCheck(result);

			result = system.getVersion(ref version);
			ErrorCheck(result);

			if (version != FMOD.VERSION.number)
				throw new ArgumentOutOfRangeException("version", version, "Unexpected version");

			result = system.init(MAX_CHANNELS, FMOD.INITFLAGS.NORMAL, (IntPtr)null);
			ErrorCheck(result);
		}

		public void PlaySong(Song song)
		{
			this.currentSong = song;

			FMOD.RESULT result;
			FMOD.Sound sound = null;

			if (channel != null)
			{
				bool playing = false;

				result = channel.isPlaying(ref playing);
				ErrorCheck(result);

				if (playing)
				{
					result = channel.getCurrentSound(ref sound);
					ErrorCheck(result);

					result = channel.stop();
					ErrorCheck(result);
				}
			}

			result = system.createSound(song.Filename, FMOD.MODE.HARDWARE, ref sound);
			ErrorCheck(result);

			result = system.playSound(FMOD.CHANNELINDEX.FREE, sound, false, ref channel);
			ErrorCheck(result);

			result = channel.setVolume(10); // 0-256
			ErrorCheck(result);

			this.OnSongStarted(song);
		}

		public void Stop()
		{
			FMOD.RESULT result;
			FMOD.Sound sound = null;

			if (channel != null)
			{
                bool playing = IsPlaying();

				if (playing)
				{
					result = channel.getCurrentSound(ref sound);
					ErrorCheck(result);

					result = channel.stop();
					ErrorCheck(result);
				}
			}
		}

        private bool IsPlaying()
        {
            if (channel == null)
                return false;

            var playing = false;

            var result = channel.isPlaying(ref playing);
            ErrorCheck(result);

            return playing;
        }

		public Song CurrentSong
		{
			get { return this.currentSong; }
		}

		public event EventHandler<SongEventArgs> SongStarted;

		private void ErrorCheck(FMOD.RESULT result)
		{
			if (result == FMOD.RESULT.OK)
				return;

			throw new FmodException(result);
		}

		private void OnSongStarted(Song song)
		{
			var handler = this.SongStarted;
			if (handler != null)
				handler(this, new SongEventArgs(song));
		}

        public MediaPlayerStatus Status
        {
            get 
            {
                return this.IsPlaying() ? MediaPlayerStatus.Playing : MediaPlayerStatus.Stopped; 
            }
        }
    }
}
