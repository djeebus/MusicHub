using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;

namespace MusicHub.ActiveDirectory
{
	public class ActiveDirectoryUserRepository : IUserRepository
	{
		static readonly Dictionary<string, User> cache = new Dictionary<string, User>();

        private User GetByConnectionId(string connectionId)
        {
            lock (cache)
            {
                foreach (var user in cache.Values)
                {
                    if (user.ConnectionIds.Contains(connectionId))
                        return user;
                }
            }

            return null;
        }

		public User Get(string username)
		{
			if (!cache.ContainsKey(username))
			{
				lock (cache)
				{
					if (!cache.ContainsKey(username))
					{
						var name = GetFullNameFromActiveDirectory(username);

						var user = new User
						{
							DisplayName = name,
							Username = username,
						};

						cache.Add(username, user);
					}
				}
			}

			return cache[username];
		}

		private static string GetFullNameFromActiveDirectory(string username)
		{
			// got from http://milanl.blogspot.com/2008/08/retrieve-full-name-from-active.html
			string strDomain;
			string strName;

			// Parse the string to check if domain name is present.
			int idx = username.IndexOf('\\');
			if (idx == -1)
			{
				idx = username.IndexOf('@');
			}

			if (idx != -1)
			{
				strDomain = username.Substring(0, idx);
				strName = username.Substring(idx + 1);
			}
			else
			{
				strDomain = Environment.MachineName;
				strName = username;
			}

			DirectoryEntry obDirEntry = null;

			obDirEntry = new DirectoryEntry("WinNT://" + strDomain + "/" + strName);
			System.DirectoryServices.PropertyCollection coll = obDirEntry.Properties;
			string name = (string)coll["FullName"].Value;
			return string.IsNullOrWhiteSpace(name) ? username : strName;
		}


		public IEnumerable<User> GetOnlineUsers()
		{
			return from u in cache.Values
				   where u.IsOnline
				   select u;
		}

        public User ClientConnected(string username, string connectionId)
        {
            var user = this.Get(username);

            if (user == null)
                throw new ArgumentOutOfRangeException("username", username, "Unknown user");

            user.IsOnline = true;

            if (user.ConnectionIds == null)
            {
                lock (user)
                {
                    if (user.ConnectionIds == null)
                        user.ConnectionIds = new List<string>();
                }
            }

            user.ConnectionIds.Add(connectionId);

            return user;
        }

        public User ClientDisconnected(string connectionId)
        {
            var user = this.GetByConnectionId(connectionId);
            if (user == null)
                return null;

            user.ConnectionIds.Remove(connectionId);

            return user;
        }
    }
}
