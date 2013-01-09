using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicHub.MicrosoftTextToSpeech
{
    public class SongAnnouncer
    {
        private readonly IJukebox _jukebox;

        public SongAnnouncer(IJukebox jukebox)
        {
            this._jukebox = jukebox;

            this._jukebox.SongStarted += _jukebox_SongStarted;
        }

        void _jukebox_SongStarted(object sender, SongEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
