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
            this.Stop();

            int streamId;
            if (mediaUrl.StartsWith("http"))
                streamId = Bass.BASS_StreamCreateURL(mediaUrl, 0, BASSFlag.BASS_DEFAULT, null, IntPtr.Zero);
            else
                streamId = Bass.BASS_StreamCreateFile(mediaUrl, 0, 0, BASSFlag.BASS_DEFAULT);

            if (streamId == 0)
                throw new BassException();

            if (!Bass.BASS_ChannelPlay(streamId, true))
                throw new BassException();

            _currentSongStreamId = streamId;

            var syncId = Bass.BASS_ChannelSetSync(streamId, BASSSync.BASS_SYNC_END, 0, _syncCallback, IntPtr.Zero);
        }

        private void SyncCallback(int handle, int channel, int data, IntPtr user)
        {
            this.OnSongFinished();
        }

        public void Stop()
        {
            if (!_currentSongStreamId.HasValue)
                return;

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
        }

        public event EventHandler SongFinished;

        private void OnSongFinished()
        {
            var handler = this.SongFinished;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            this.Stop();

            Bass.BASS_Free();
        }
    }
}
