using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;
using System.Text;

namespace MusicHub.MicrosoftTextToSpeech
{
    public class SongAnnouncer
    {
        private readonly IJukebox _jukebox;
        private readonly SpeechSynthesizer _speaker;

        private readonly SpeechAudioFormatInfo _audioFormat = new SpeechAudioFormatInfo(
            32000,
            AudioBitsPerSample.Sixteen,
            AudioChannel.Mono);


        public SongAnnouncer(IJukebox jukebox)
        {
            this._jukebox = jukebox;

            this._jukebox.SongStarting += _jukebox_SongStarting;

            this._speaker = new SpeechSynthesizer();
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

        void _jukebox_SongStarting(object sender, SongEventArgs e)
        {
            var script = GetScript(e.Song);

            var prompt = new Prompt(script, SynthesisTextFormat.Text);

            var tempFile = Path.GetTempFileName();

            using (var outputStream = File.OpenWrite(tempFile))
            {
                this._speaker.SetOutputToAudioStream(
                    outputStream,
                    this._audioFormat);

                _speaker.Speak(prompt);
            }

            File.Delete(tempFile);
            this._speaker.SetOutputToNull();
        }
    }
}