using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MusicHub
{
	public interface IAffinityTracker
	{
		void Record(string userId, string songId, Affinity affinity);
        string[] GetMostLovedArtists();
        string[] GetMostHatedArtists();

        User[] GetMostLovedUsers();
        User[] GetMostHatedUsers();

        User[] GetMostLovingUsers();
        User[] GetMostHatingUsers();
    }

	public enum Affinity
	{
		Love,
		Hate,
	}
}
