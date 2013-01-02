using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicHub
{
    public class LibraryInfo
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }
        public string Location { get; set; }

        public LibraryType Type { get; set; }

        public long TotalSongs { get; set; }
    }
}
