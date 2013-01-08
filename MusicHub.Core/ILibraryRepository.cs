using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicHub
{
    public interface ILibraryRepository
    {
        LibraryInfo[] GetLibraries();
        LibraryInfo[] GetLibrariesForUser(string userId);
        LibraryInfo GetLibrary(string libraryId);
        LibraryInfo Create(string userId, LibraryType type, string path, string username, string password);
        void Delete(string libraryId);
        void UpdateLastSyncDate(string libraryId);
        void MarkAsPlayed(string libraryId);
    }
}
