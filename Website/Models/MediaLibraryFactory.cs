using MusicHub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website.Models
{
    public class MediaLibraryFactory
    {
        private readonly ILibraryRepository _libraryRepository;
        private readonly IMetadataService _metadataService;

        public MediaLibraryFactory(
            ILibraryRepository libraryRepository,
            IMetadataService metadataService)
        {
            _libraryRepository = libraryRepository;
            _metadataService = metadataService;
        }

        public IMusicLibrary GetLibrary(string libraryId)
        {
            var library = _libraryRepository.GetLibrary(libraryId);
            if (library == null)
                throw new ArgumentOutOfRangeException("libraryId");

            switch (library.Type)
            {
                case LibraryType.SharedFolder:
                    return new MusicHub.Implementation.FileSystemMusicLibrary(
                        library.Id,
                        library.Location, 
                        _metadataService);

                default:
                    throw new NotImplementedException();
            }
        }
    }
}