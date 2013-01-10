using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Un4seen.Bass.AddOn.Enc;
using Un4seen.Bass.AddOn.Mix;

namespace MusicHub.BassNet
{
    public class BassNetHttpServer
    {
        int port = 8888;

        ENCODECLIENTPROC myClientProc;

        private readonly int _encoderHandle;
        private readonly int _serverHandle;

        public BassNetHttpServer()
        {
        }

        public void PlayStream(int streamId)
        {
            BassMix.BASS_Mixer_StreamAddChannel(
                streamId, 0, Un4seen.Bass.BASSFlag.BASS_DEFAULT);
        }

        private bool clientProc(int handle, bool connect, string client, IntPtr headers, IntPtr user)
        {
            throw new NotImplementedException();
        }
    }
}
