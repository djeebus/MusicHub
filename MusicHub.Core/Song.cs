using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MusicHub
{
    [DebuggerDisplay("{Title} by {Artist}")]
    public class Song
    {
        public string Id { get; set; }
        public string UserName { get; set; }

        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public uint? Track { get; set; }
        public uint? Year { get; set; }

		public string Filename { get; set; }
	}
}
