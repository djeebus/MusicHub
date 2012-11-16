using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicHub
{
    public interface IMetadataService
    {
        Song GetSongFromFilename(string filename);
    }
}