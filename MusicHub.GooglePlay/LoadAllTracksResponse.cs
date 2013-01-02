using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace MusicHub.GooglePlay
{
    public class LoadAllTracksResponse
    {
        [JsonProperty("continuation")]
        public bool IsContinuation { get; set; }

        [JsonProperty("differentialUpdate")]
        public bool IsDifferentialUpdate { get; set; }

        [JsonProperty("continuationToken")]
        public string ContinuationToken { get; set; }

        [JsonProperty("playlist")]
        public Song[] Songs { get; set; }

        [JsonProperty("playlistID")]
        public string PlaylistId { get; set; }

        [JsonProperty("requestTime")]
        public string RequestTime { get; set; }
    }
}
