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
        public string ExternalId { get; set; }
        public string LibraryId { get; set; }

        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public uint? Track { get; set; }
        public uint? Year { get; set; }
    }
}
