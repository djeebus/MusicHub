using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicHub.FMod
{
	class FmodException : Exception
	{
		public FMOD.RESULT Result { get; private set; }

		public FmodException(FMOD.RESULT result)
			: base(string.Format("{0}: {1}", result, FMOD.Error.String(result)))
		{
			this.Result = result;
		}
	}
}
