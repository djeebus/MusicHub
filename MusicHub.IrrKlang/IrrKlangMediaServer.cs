using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ik = IrrKlang;

namespace MusicHub.IrrKlang
{
    public class IrrKlangMediaServer : IMediaServer, IDisposable
    {
		readonly object locker = new object();

        readonly ik.ISoundEngine engine = new ik.ISoundEngine();
        ik.ISound currentSound;

        public void PlaySong(string filename)
        {
			lock (locker)
			{
				if (currentSound != null)
				{
					currentSound.Stop();
					currentSound.Dispose();
				}

				currentSound = engine.Play2D(filename, false, false);
			}
        }

		public void Dispose()
		{
			if (engine != null)
			{
				engine.Dispose();
			}

			if (currentSound != null)
			{
				currentSound.Dispose();
			}
		}
	}
}
