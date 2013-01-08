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

        public string Username { get; set; }

        public override string ToString()
        {
            string format;

            if (this.Album != null && this.Year.HasValue)
                format = "{4} is playing {0} by {1} from {2}, released in {3}";
            else if (this.Album != null)
                format = "{4} is playing {0} by {1} from {2}";
            else 
                format = "{4} is playing {0} by {1}";

            return string.Format(format,
                this.Title,
                this.Artist,
                this.Album,
                this.Year,
                this.Username);
        }
    }
}
