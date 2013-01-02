using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicHub.GooglePlay
{
    public class Song
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("year")]
        public uint? Year { get; set; }

        [JsonProperty("track")]
        public uint? Track { get; set; }

        [JsonProperty("album")]
        public string Album { get; set; }

        [JsonProperty("rating")]
        public int Rating { get; set; }
    }
}
