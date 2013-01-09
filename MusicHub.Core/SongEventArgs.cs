using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicHub
{
    public class SongEventArgs : EventArgs
    {
        public Song Song { get; private set; }

        public SongEventArgs(Song song)
        {
            this.Song = song;
        }
    }
}