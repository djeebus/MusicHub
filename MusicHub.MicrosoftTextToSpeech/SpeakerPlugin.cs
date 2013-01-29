using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;

namespace MusicHub.MicrosoftTextToSpeech
{
    public class SongAnnouncer
    {
        private readonly IJukebox _jukebox;
        private readonly IMediaPlayer _mediaPlayer;

        private readonly SpeechAudioFormatInfo _audioFormat = new SpeechAudioFormatInfo(
            48000,
            AudioBitsPerSample.Sixteen,
            AudioChannel.Stereo);
        //private readonly SpeechAudioFormatInfo _audioFormat = new SpeechAudioFormatInfo(
        //    EncodingFormat.Pcm,
        //    48000,
        //    16,
        //    2,
        //    320 * (1024 / 8),
        //    32,
        //    new byte[0]);


        public SongAnnouncer(IJukebox jukebox, IMediaPlayer mediaPlayer)
        {
            this._jukebox = jukebox;
            this._mediaPlayer = mediaPlayer;

            this._jukebox.SongStarting += _jukebox_SongStarting;
        }

        string[] _scripts = new[] {
            "Now playing: {Title}, by {Artist}",
        };

        static readonly Random _random = new Random();
        private string GetScript(Song song)
        {
            var max = _scripts.Length;

            var next = _random.Next(0, max);

            var script = _scripts[next];

            var sb = new StringBuilder(script);

            sb.Replace("{Title}", song.Title);
            sb.Replace("{Artist}", song.Artist);

            return sb.ToString();
        }

        private string _lastTempFile = null;
        void _jukebox_SongStarting(object sender, SongEventArgs e)
        {
            if (_lastTempFile != null)
            {
                if (File.Exists(_lastTempFile))
                    File.Delete(_lastTempFile);

                _lastTempFile = null;
            }

            var script = GetScript(e.Song);

            var prompt = new Prompt(script, SynthesisTextFormat.Text);

            var tempFile = Path.GetTempFileName();

            using (var speaker = new SpeechSynthesizer())
            {
                speaker.SetOutputToWaveFile(
                    tempFile,
                    this._audioFormat);

                speaker.Speak(prompt);
            }

            this._mediaPlayer.PlaySong(null, tempFile);

            using (var mre = new ManualResetEvent(false))
            {
                var handler = new EventHandler((s, args) => mre.Set());

                this._mediaPlayer.SongFinished += handler;
                try
                {
                    this._mediaPlayer.PlaySong(null, tempFile);
                    mre.WaitOne();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(string.Format("Error playing prompt: {0}", ex), "SongAnnouncer");
                }
                finally
                {
                    this._mediaPlayer.SongFinished -= handler;
                }
            }

            _lastTempFile = tempFile;
        }
    }
}