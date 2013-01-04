using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicHub.ConsoleApp
{
    public class MusicLibraryFactory : IMusicLibraryFactory
    {
        private readonly IMetadataService _metadataService;

        public MusicLibraryFactory(IMetadataService metadataService)
        {
            _metadataService = metadataService;
        }

        public IMusicLibrary Create(LibraryInfo libraryInfo)
        {
            switch (libraryInfo.Type)
            {
                case LibraryType.GoogleMusic:
                    return new GooglePlay.GoogleMusicMusicLibrary(libraryInfo.Username, libraryInfo.Password);

                case LibraryType.SharedFolder:
                    return new Implementation.FileSystemMusicLibrary(libraryInfo.Location, _metadataService);

                default:
                    throw new ArgumentOutOfRangeException("libraryInfo.Type", libraryInfo.Type, "Unknown type");
            }
        }
    }
}