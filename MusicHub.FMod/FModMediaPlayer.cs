using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace MusicHub.FMod
{
    public class FModMediaPlayer : IMediaPlayer, IDisposable
    {
        const int MAX_CHANNELS = 32;

        private readonly FMOD.System system;
        private readonly FMOD.CHANNEL_CALLBACK channel_callback;

        private FMOD.Channel channel;
        private Song currentSong;

        private FMOD.FILE_OPENCALLBACK myopen;
        private FMOD.FILE_CLOSECALLBACK myclose;
        private FMOD.FILE_READCALLBACK myread;
        private FMOD.FILE_SEEKCALLBACK myseek;

        public FModMediaPlayer()
        {
            myopen = new FMOD.FILE_OPENCALLBACK(OPENCALLBACK);
            myclose = new FMOD.FILE_CLOSECALLBACK(CLOSECALLBACK);
            myread = new FMOD.FILE_READCALLBACK(READCALLBACK);
            myseek = new FMOD.FILE_SEEKCALLBACK(SEEKCALLBACK);

            FMOD.RESULT result;

            uint version = 0;

            result = FMOD.Factory.System_Create(ref system);
            ErrorCheck(result, "System_Create");

            result = system.getVersion(ref version);
            ErrorCheck(result, "system.getVersion");

            if (version != FMOD.VERSION.number)
                throw new ArgumentOutOfRangeException("version", version, "Unexpected version");

            result = system.init(MAX_CHANNELS, FMOD.INITFLAGS.NORMAL, (IntPtr)null);
            ErrorCheck(result, "system.init");

            result = system.setFileSystem(myopen, myclose, myread, myseek, null, null, 2048);
            ErrorCheck(result, "system.setFileSystem");

            channel_callback = channelCallbackHandler;
        }

        static readonly object _locker = new object();
        public void PlaySong(Song song, string mediaLocation)
        {
            this.currentSong = song;

            lock (_locker)
                this.PlayInternal(song, mediaLocation);

            this.OnSongStarted(song);

            this.OnStatusChanged(MediaPlayerStatus.Playing);
        }

        int index = 0;
        Dictionary<int, Tuple<Stream, HttpWebResponse>> _cache = new Dictionary<int, Tuple<Stream, HttpWebResponse>>();
        private FMOD.RESULT OPENCALLBACK([MarshalAs(UnmanagedType.LPWStr)]string name, int unicode, ref uint filesize, ref IntPtr handle, ref IntPtr userdata)
        {
            var tmp = Interlocked.Increment(ref index);

            userdata = (IntPtr)tmp;

            Tuple<Stream, HttpWebResponse> tuple;

            if (name.StartsWith("http"))
            {
                var request = WebRequest.Create(name);

                var response = (HttpWebResponse)request.GetResponse();

                var stream = response.GetResponseStream();

                filesize = (uint)response.ContentLength;

                tuple = new Tuple<Stream, HttpWebResponse>(stream, response);
            }
            else
            {
                var stream = new FileStream(name, FileMode.Open, FileAccess.Read);
                filesize = (uint)stream.Length;
                tuple = new Tuple<Stream, HttpWebResponse>(stream, null);
            }

            lock (_cache)
                _cache.Add(tmp, tuple);

            return FMOD.RESULT.OK;
        }

        private FMOD.RESULT CLOSECALLBACK(IntPtr handle, IntPtr userdata)
        {
            int tmp = (int)userdata;

            var result = _cache[tmp];

            if (result.Item1 != null)
            {
                using (result.Item1) { }
                Trace.WriteLine("CLOSECALLBACK: Closed stream", "FModMediaPlayer");
            }

            if (result.Item2 != null)
            {
                using (result.Item2) { }
                Trace.WriteLine("CLOSECALLBACK: Closed response", "FModMediaPlayer");
            }

            lock (_cache)
                _cache.Remove(tmp);

            return FMOD.RESULT.OK;
        }

        private FMOD.RESULT READCALLBACK(IntPtr handle, IntPtr buffer, uint sizebytes, ref uint bytesread, IntPtr userdata)
        {
            int tmp = (int)userdata;

            var result = _cache[tmp];

            var stream = result.Item1;

            byte[] readbuffer = new byte[sizebytes];

            bytesread = (uint)stream.Read(readbuffer, 0, (int)sizebytes);
            if (bytesread == 0)
            {
                return FMOD.RESULT.ERR_FILE_EOF;
            }

            Marshal.Copy(readbuffer, 0, buffer, (int)sizebytes);

            return FMOD.RESULT.OK;
        }

        private FMOD.RESULT SEEKCALLBACK(IntPtr handle, int pos, IntPtr userdata)
        {
            int tmp = (int)userdata;

            var result = _cache[tmp];

            var stream = result.Item1;

            if (pos != 0)
                throw new ArgumentOutOfRangeException("pos", pos, "Unable to seek");

            return FMOD.RESULT.OK;
        }

        private void PlayInternal(Song song, string mediaLocation)
        {
            FMOD.RESULT result;
            FMOD.Sound sound = null;

            this.StopInternal();

            //var info = new FMOD.CREATESOUNDEXINFO
            //{
            //    suggestedsoundtype = FMOD.SOUND_TYPE.MPEG,
            //};

            result = system.createSound(mediaLocation, FMOD.MODE._2D | FMOD.MODE.HARDWARE | FMOD.MODE.CREATESTREAM | FMOD.MODE.IGNORETAGS, ref sound);
            ErrorCheck(result, "system.createSound");

            result = system.playSound(FMOD.CHANNELINDEX.FREE, sound, false, ref channel);
            ErrorCheck(result, "system.playSound");

            result = channel.setVolume(10); // 0-256
            ErrorCheck(result, "channel.setVolume");

            channel.setCallback(channel_callback);
        }

        private FMOD.RESULT channelCallbackHandler(IntPtr channelraw, FMOD.CHANNEL_CALLBACKTYPE type, IntPtr commanddata1, IntPtr commanddata2)
        {
            switch (type)
            {
                case FMOD.CHANNEL_CALLBACKTYPE.END:
                    this.OnStatusChanged(MediaPlayerStatus.SongFinished);
                    break;
            }

            return FMOD.RESULT.OK;
        }

        public void Stop()
        {
            bool result;
            lock (_locker)
                result = StopInternal();

            if (!result)
                return;

            this.OnStatusChanged(MediaPlayerStatus.Stopped);
        }

        private bool StopInternal()
        {
            FMOD.RESULT result;
            FMOD.Sound sound = null;

            if (channel == null)
                return false;

            bool playing = IsPlaying();

            if (!playing)
                return false;

            result = channel.getCurrentSound(ref sound);
            ErrorCheck(result, "channel.getCurrentSound");

            result = channel.stop();
            ErrorCheck(result, "channel.stop");

            channel = null;
            result = sound.release();
            ErrorCheck(result, "sound.release");

            return true;
        }

        private bool IsPlaying()
        {
            lock (_locker)
            {
                if (channel == null)
                    return false;

                var playing = false;

                var result = channel.isPlaying(ref playing);
                ErrorCheck(result, "channel.isPlaying");

                return playing;
            }
        }

        public Song CurrentSong
        {
            get { return this.currentSong; }
        }

        public event EventHandler<SongEventArgs> SongStarted;

        private void ErrorCheck(FMOD.RESULT result, string action)
        {
            Trace.WriteLine(string.Format("{0}: {1}", action, result), "FModMediaServer");

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

        public event EventHandler<StatusEventArgs> StatusChanged;

        private void OnStatusChanged(MediaPlayerStatus status)
        {
            var handler = this.StatusChanged;
            if (handler != null)
                handler(this, new StatusEventArgs(status));
        }

        public void Dispose()
        {
            foreach (var kvp in _cache)
            {
                var stream = kvp.Value.Item1;
                var response = kvp.Value.Item2;

                if (stream != null)
                {
                    using (stream) { }
                    Trace.WriteLine("Dispose: Closed stream", "FModMediaPlayer");
                }

                if (response != null)
                {
                    using (response) { }
                    Trace.WriteLine("Dispose: Closed response", "FModMediaPlayer");
                }
            }
        }
    }
}
