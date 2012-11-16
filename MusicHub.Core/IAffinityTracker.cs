using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicHub
{
	public interface IAffinityTracker
	{
		void Record(User user, Song song, Affinity affinity);
	}

	public enum Affinity
	{
		Love,
		Hate,
	}
}
