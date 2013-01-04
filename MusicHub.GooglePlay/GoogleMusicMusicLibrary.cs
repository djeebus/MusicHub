using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicHub.GooglePlay
{
    public class GoogleMusicMusicLibrary : IMusicLibrary
    {
        private readonly string _username, _password;

        private readonly GoogleMusicClient _client;

        public GoogleMusicMusicLibrary(string username, string password)
        {
            this._username = username;
            this._password = password;

            _client = new GoogleMusicClient();

            _client.LogOn(_username, _password);
        }

        public IEnumerable<MusicHub.Song> GetSongs()
        {
            string continuationToken = null;

            do
            {
                var result = _client.GetLibrary(ref continuationToken);

                foreach (var song in result)
                {
                    switch (song.Rating)
                    {
                        case Constants.RATING_Bad:
                            continue;

                        case Constants.RATING_Good:
                            break;

                        case Constants.RATING_Unrated:
                            break;

                        default:
                            Trace.WriteLine(string.Format("Unknown rating: {0}", song.Rating), "GoogleMusicMusicLibrary");
                            continue;
                    }

                    yield return new MusicHub.Song
                    {
                        Album = song.Album,
                        Artist = song.Artist,
                        ExternalId = song.Id,
                        Title = song.Title,
                        Track = song.Track,
                        Year = song.Year,
                    };
                }
            } while (!string.IsNullOrWhiteSpace(continuationToken));
        }

        public string GetSongUrl(string externalId)
        {
            return _client.GetSongUrl(externalId).ToString();
        }
    }
}
