using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicHub
{
    public interface IJukebox
    {
        Song CurrentSong { get; }

        event EventHandler<SongEventArgs> SongStarted;
        event EventHandler<SongEventArgs> SongFinished;

        void Play();

        HateResult Hate(string userId);
        void Love(string userId);

        void SkipTrack();

        void UpdateLibrary(string libraryId);

        void CreateLibrary(string userId, LibraryType libraryType, string path, string username, string password);

        void DeleteLibrary(string libraryId);

        LibraryInfo[] GetLibrariesForUser(string userId);

        void Stop();

        Song[] FindSongs(SearchType type, string term);
    }
}
