using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Un4seen.Bass;

namespace MusicHub.BassNet
{
    public class BassException : Exception
    {
        public BassException()
            : base(string.Format("Bass error: {0}", Bass.BASS_ErrorGetCode()))
        {
        }
    }
}
