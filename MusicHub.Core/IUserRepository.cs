using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicHub
{
	public interface IUserRepository
	{
        User GetById(string userId);
        User[] GetOnlineUsers();
        void MarkAsOnline(string userId, bool isOnline);

        User EnsureUser(string username, string displayName);
    }
}
