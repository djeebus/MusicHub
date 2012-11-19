using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicHub
{
    public interface ILibraryRepository
    {
        Library[] GetLibrariesForUser(string userId);
        void Create(string userId, string path);
    }
}
