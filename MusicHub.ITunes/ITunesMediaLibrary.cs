using DAAP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MusicHub.ITunes
{
    public class ITunesMediaLibrary : IMusicLibrary
    {
        private readonly LibraryInfo _info;

        private readonly Client _client;

        private readonly string _tempFilename;

        public ITunesMediaLibrary(LibraryInfo info)
        {
            this._info = info;

            this._client = new Client(info.Location, 3689);
            this._client.Login();

            _tempFilename = Path.GetTempFileName();
            File.Delete(_tempFilename); // previous method actually creates the file
        }

        public IEnumerable<Song> GetSongs()
        {
            foreach (var database in this._client.Databases)
            {
                foreach (var track in database.Tracks)
                {
                    yield return new Song
                    {
                        Album = track.Album,
                        Artist = track.Artist,
                        ExternalId = track.Id.ToString(),
                        Id = null,
                        LibraryId = _info.Id,
                        Title = track.Title,
                        Track = track.TrackNumber <= 0 ? (uint?)null : (uint?)track.TrackNumber,
                        UserId = null,
                        Username = _info.Username,
                        Year = track.Year <= 0 ? (uint?)null : (uint)track.Year,
                    };
                }
            }
        }

        const char PartSeperator = '|';
        static readonly string ExternalIdFormat = "{0}" + PartSeperator + "{1}" + PartSeperator + "{2}";

        private static string FromDaap(Database database, Track track)
        {
            return string.Format(
                ExternalIdFormat,
                database.Id,
                track.Id,
                track.Format);
        }

        private static void ToDaap(string externalId, out int databaseId, out int trackId, out string trackFormat)
        {
            var parts = externalId.Split(PartSeperator);
            if (parts.Length != ExternalIdFormat.Count(e => e == '{'))
                throw new ArgumentOutOfRangeException("externalId", externalId, "Wrong number of parts");

            databaseId = int.Parse(parts[0]);
            trackId = int.Parse(parts[1]);
            trackFormat = parts[2];
        }        

        public string GetSongUrl(string externalId)
        {
            string format;
            int databaseId, trackId;

            ToDaap(externalId, out databaseId, out trackId, out format);
            
            var database = this._client.Databases.FirstOrDefault(d => d.Id == databaseId);

            var track = new Track();
            track.SetId(trackId);
            track.Format = format;

            database.DownloadTrack(track, _tempFilename);

            return _tempFilename;
        }
    }
}
