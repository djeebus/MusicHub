using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicHub
{
    public interface ILibraryRepository
    {
        Library[] GetLibrariesForUser(string userId);
        void Create(string userId, LibraryType type, string path, string username, string password);

        void Delete(string libraryId);
    }
}
