using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicHub
{
	public class User
	{
		public string Username { get; set; }
		public string DisplayName { get; set; }
		public bool IsOnline { get; set; }
        public List<string> ConnectionIds { get; set; }
	}
}
