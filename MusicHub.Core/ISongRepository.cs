using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicHub
{
    public interface ISongRepository
    {
        Song UpsertSong(string libraryId, string externalId, string artist, string title, string album, uint? track, uint? year);

        Song GetRandomSong(Song previousSong);

        void PruneSongs(string libraryId);
    }
}
