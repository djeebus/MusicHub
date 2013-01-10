using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Un4seen.Bass;

namespace MusicHub.BassNet
{
    public class BassException : Exception
    {
        public BassException(int errorCode)
            : this((BASSError)errorCode)
        {
        }

        public BassException()
            : this(Bass.BASS_ErrorGetCode())
        {
        }

        protected BassException(BASSError bassError)
            : base(string.Format("Bass error: {0}", bassError))
        {
        }
    }
}
