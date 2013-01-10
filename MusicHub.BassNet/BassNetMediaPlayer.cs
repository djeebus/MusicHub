using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using global::Un4seen.Bass;
using System.IO;
using System.Diagnostics;
using Un4seen.Bass.AddOn.Enc;
using Un4seen.Bass.AddOn.Mix;

namespace MusicHub.BassNet
{
    public class BassNetMediaPlayer : IMediaPlayer, IDisposable
    {
        int port = 8888;

        ENCODECLIENTPROC myClientProc;

        private int _encoderHandle;
        private int _serverHandle;
        private int _mixerStreamId;


        public BassNetMediaPlayer()
        {
            InitBass();
            InitBassMixer();
            InitBassEncoder();
            InitBassServer();

            _syncCallback = SyncCallback;
            myClientProc = clientProc;
        }

        private static void InitBass()
        {
            var currentFolder = Directory.GetCurrentDirectory();
            var pluginFolder = Path.Combine(currentFolder, "plugins");

            foreach (var plugin in Directory.GetDirectories(currentFolder, "bass*.dll"))
            {
                if (Path.GetFileName(plugin).ToLower() == "bass.dll")
                    continue;

                var errorCode = Bass.BASS_PluginLoad(plugin);
                if (errorCode != 0)
                    throw new BassException(errorCode);
            }

            if (!Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero))
                throw new BassException();
        }

        private void InitBassEncoder()
        {
            var currentFolder = Directory.GetCurrentDirectory();
            var lamePath = Path.Combine(currentFolder, "lame.exe");

            _encoderHandle = BassEnc.BASS_Encode_Start(
                _mixerStreamId,
                string.Format(@"{0} -r -s 44100 -b 128 -", lamePath),
                BASSEncode.BASS_ENCODE_NOHEAD,
                null,
                IntPtr.Zero);

            if (_encoderHandle == 0)
                throw new BassException();
        }

        private void InitBassMixer()
        {
            
            _mixerStreamId = BassMix.BASS_Mixer_StreamCreate(
                44100,
                2,
                BASSFlag.BASS_DEFAULT);
            if (_mixerStreamId == 0)
                throw new BassException();

            if (!Bass.BASS_ChannelPlay(_mixerStreamId, false))
                throw new BassException();           
        }

        private void InitBassServer()
        {
            this._serverHandle = BassEnc.BASS_Encode_ServerInit(
                _encoderHandle,
                port.ToString(),
                64 * 1024,
                64 * 1024,
                BASSEncodeServer.BASS_ENCODE_SERVER_DEFAULT,
                null,
                IntPtr.Zero);

            if (this._serverHandle == 0)
                throw new BassException();
        }

        private bool clientProc(int handle, bool connect, string client, IntPtr headers, IntPtr user)
        {
            return true;
        }

        private readonly SYNCPROC _syncCallback;
        int? _currentSongStreamId = null;

        public void PlaySong(Song song, string mediaUrl)
        {
            this.Stop();

            int streamId;
            if (mediaUrl.StartsWith("http"))
                streamId = Bass.BASS_StreamCreateURL(mediaUrl, 0, BASSFlag.BASS_STREAM_DECODE, null, IntPtr.Zero);
            else
                streamId = Bass.BASS_StreamCreateFile(mediaUrl, 0, 0, BASSFlag.BASS_STREAM_DECODE);

            if (streamId == 0)
                throw new BassException();

            if (!BassMix.BASS_Mixer_StreamAddChannel(_mixerStreamId, streamId, BASSFlag.BASS_DEFAULT))
                throw new BassException();

            _currentSongStreamId = streamId;

            var syncId = Bass.BASS_ChannelSetSync(streamId, BASSSync.BASS_SYNC_END, 0, _syncCallback, IntPtr.Zero);
            if (syncId == 0)
                throw new BassException();
        }

        private void SyncCallback(int handle, int channel, int data, IntPtr user)
        {
            this.OnSongFinished();
        }

        public void Stop()
        {
            if (!_currentSongStreamId.HasValue)
                return;

            if (!BassMix.BASS_Mixer_ChannelRemove(_currentSongStreamId.Value))
                throw new BassException();

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
