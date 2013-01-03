using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using global::Un4seen.Bass;
using System.IO;
using System.Diagnostics;

namespace MusicHub.BassNet
{
    public class BassNetMediaPlayer : IMediaPlayer, IDisposable
    {
        public Song CurrentSong
        {
            get;
            private set;
        }

        public MediaPlayerStatus Status
        {
            get 
            { 
                if (!_currentSongStreamId.HasValue)
                    return MediaPlayerStatus.Stopped;

                var state = Bass.BASS_ChannelIsActive(_currentSongStreamId.Value);
                switch (state)
                {
                    case BASSActive.BASS_ACTIVE_PLAYING:
                        return MediaPlayerStatus.Playing;

                    default:
                        return MediaPlayerStatus.Stopped;
                }
            }
        }

        public BassNetMediaPlayer()
        {
            if (!Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero))
                throw new BassException();

            _syncCallback = SyncCallback;
        }

        private readonly SYNCPROC _syncCallback;
        int? _currentSongStreamId = null;

        public void PlaySong(Song song, string mediaUrl)
        {
            var result = this.StopInternal();

            this.CurrentSong = song;

            int streamId;
            if (mediaUrl.StartsWith("http"))
                streamId = Bass.BASS_StreamCreateURL(mediaUrl, 0, BASSFlag.BASS_DEFAULT, null, IntPtr.Zero);
            else
                streamId = Bass.BASS_StreamCreateFile(mediaUrl, 0, 0, BASSFlag.BASS_DEFAULT);

            if (!Bass.BASS_ChannelPlay(streamId, true))
                throw new BassException();

            _currentSongStreamId = streamId;

            var syncId = Bass.BASS_ChannelSetSync(streamId, BASSSync.BASS_SYNC_END, 0, _syncCallback, IntPtr.Zero);

            this.OnSongStarted(song);

            if (!result)
                this.OnStatusChanged(MediaPlayerStatus.Playing);
        }

        private void SyncCallback(int handle, int channel, int data, IntPtr user)
        {
            this.OnStatusChanged(MediaPlayerStatus.SongFinished);
        }

        public void Stop()
        {
            var result = StopInternal();

            if (result)
                this.OnStatusChanged(MediaPlayerStatus.Stopped);
        }

        private bool StopInternal()
        {
            if (!_currentSongStreamId.HasValue)
                return false;

            var active = Bass.BASS_ChannelIsActive(_currentSongStreamId.Value);

            switch (active)
            {
                case BASSActive.BASS_ACTIVE_STOPPED:
                    break;

                default:
                    if (!Bass.BASS_ChannelStop(_currentSongStreamId.Value))
                        throw new BassException();

                    if (!Bass.BASS_StreamFree(_currentSongStreamId.Value))
                        throw new BassException();
                    break;
            }

            _currentSongStreamId = null;

            return true;
        }

        public event EventHandler<SongEventArgs> SongStarted;

        private void OnSongStarted(Song song)
        {
            var handler = this.SongStarted;
            if (handler != null)
                handler(this, new SongEventArgs(song));
        }

        public event EventHandler<StatusEventArgs> StatusChanged;

        private void OnStatusChanged(MediaPlayerStatus status)
        {
            var handler = this.StatusChanged;
            if (handler != null)
                handler(this, new StatusEventArgs(status));
        }

        public void Dispose()
        {
            this.StopInternal();

            Bass.BASS_Free();
        }
    }
}
