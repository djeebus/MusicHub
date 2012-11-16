using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicHub
{
	public interface IUserRepository
	{
		User Get(string username);
		User ClientConnected(string username, string connectionId);
        User ClientDisconnected(string connectionId);

		IEnumerable<User> GetOnlineUsers();
	}
}
