using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;

namespace MusicHub.ActiveDirectory
{
	public class ActiveDirectoryAuthenticationService : IAuthenticationService
	{
		static readonly Dictionary<string, User> cache = new Dictionary<string, User>();

        public User GetDetails(string username)
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
    }
}
