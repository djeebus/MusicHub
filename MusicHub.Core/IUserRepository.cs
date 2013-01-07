using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicHub
{
	public interface IUserRepository
	{
		//User GetByName(string username);
        User GetById(string userId);
        User[] GetOnlineUsers();

        //User Create(string username, string displayName);
        User EnsureUser(string username, string displayName);
    }
}
